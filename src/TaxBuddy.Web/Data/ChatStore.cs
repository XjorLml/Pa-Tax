using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using TaxBuddy.Web.Application.Chat;
using TaxBuddy.Web.Application.Directory;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Data;

/// <summary>Conversations and messages on PostgreSQL (F07). A new context per operation (SDD), so interactive pages can share it.</summary>
public sealed class ChatStore(IDbContextFactory<TaxBuddyDbContext> contextFactory, ILogger<ChatStore> logger) : IChatStore
{
    private static readonly EventId DataUnavailable = new(1004, "ChatDataUnavailable");

    /// <summary>Characters of the last message shown in the conversation list.</summary>
    private const int PreviewLength = 140;

    public Task<(AccountantId ProfileId, Guid? OwnerAccountId, string Name, string? PictureObject)?> FindListedAccountantAsync(
        AccountantId profileId, CancellationToken cancellationToken) =>
        Run(async db =>
        {
            var profile = await db.AccountantProfiles.AsNoTracking()
                .Where(p => p.Id == profileId)
                .Where(ListingRules.IsListed)
                .Select(p => new
                {
                    p.Id,
                    p.OwnerAccountId,
                    p.DisplayName,
                    Picture = db.Users.Where(u => u.Id == p.OwnerAccountId).Select(u => u.PictureObject).FirstOrDefault(),
                })
                .FirstOrDefaultAsync(cancellationToken);
            return profile is null
                ? ((AccountantId, Guid?, string, string?)?)null
                : (profile.Id, profile.OwnerAccountId, profile.DisplayName, profile.Picture);
        }, cancellationToken);

    public Task<Guid?> FindConversationIdAsync(Guid clientAccountId, Guid accountantAccountId, CancellationToken cancellationToken) =>
        Run(db => db.Conversations.AsNoTracking()
            .Where(c => c.ClientAccountId == clientAccountId && c.AccountantAccountId == accountantAccountId)
            .Select(c => (Guid?)c.Id)
            .FirstOrDefaultAsync(cancellationToken), cancellationToken);

    public Task<Conversation?> FindAsync(Guid conversationId, CancellationToken cancellationToken) =>
        Run(db => db.Conversations.AsNoTracking().FirstOrDefaultAsync(c => c.Id == conversationId, cancellationToken), cancellationToken);

    public Task<ChatMessage?> FindMessageAsync(Guid messageId, CancellationToken cancellationToken) =>
        Run(db => db.Messages.AsNoTracking().FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken), cancellationToken);

    public Task<IReadOnlyList<ConversationRow>> ListAsync(Guid accountId, CancellationToken cancellationToken) =>
        Run<IReadOnlyList<ConversationRow>>(async db =>
        {
            var rows = await db.Conversations.AsNoTracking()
                .Where(c => c.ClientAccountId == accountId || c.AccountantAccountId == accountId)
                .OrderByDescending(c => c.LastMessageAt)
                .Select(c => new
                {
                    c.Id,
                    PeerId = c.ClientAccountId == accountId ? c.AccountantAccountId : c.ClientAccountId,
                    ReadAt = c.ClientAccountId == accountId ? c.ClientReadAt : c.AccountantReadAt,
                })
                .Select(c => new
                {
                    c.Id,
                    c.PeerId,
                    ProfileName = db.AccountantProfiles.Where(p => p.OwnerAccountId == c.PeerId).Select(p => p.DisplayName).FirstOrDefault(),
                    Peer = db.Users.Where(u => u.Id == c.PeerId).Select(u => new { u.DisplayName, u.PictureObject }).FirstOrDefault(),
                    Last = db.Messages.Where(m => m.ConversationId == c.Id)
                        .OrderByDescending(m => m.SentAt)
                        .Select(m => new { Text = m.Text.Substring(0, PreviewLength), m.SenderAccountId, m.SentAt })
                        .FirstOrDefault(),
                    Unread = db.Messages.Count(m => m.ConversationId == c.Id && m.SenderAccountId != accountId
                                                    && (c.ReadAt == null || m.SentAt > c.ReadAt)),
                })
                .ToListAsync(cancellationToken);

            return rows.Select(r => new ConversationRow(
                    r.Id,
                    r.PeerId,
                    PeerName(r.ProfileName, r.Peer?.DisplayName),
                    r.Peer?.PictureObject,
                    r.Last?.Text,
                    r.Last?.SenderAccountId,
                    r.Last?.SentAt,
                    r.Unread))
                .ToList();
        }, cancellationToken);

    public Task<ThreadData?> LoadThreadAsync(Guid conversationId, Guid viewerId, int limit, CancellationToken cancellationToken) =>
        Run(async db =>
        {
            var conversation = await db.Conversations.AsNoTracking().FirstOrDefaultAsync(c => c.Id == conversationId, cancellationToken);
            if (conversation is null || !conversation.Includes(viewerId))
            {
                return null; // No messages are read for anyone outside the conversation (BR05).
            }

            var peerId = conversation.OtherThan(viewerId);
            var peer = await db.Users.AsNoTracking().Where(u => u.Id == peerId)
                .Select(u => new { u.DisplayName, u.PictureObject, u.DeletedAt }).FirstOrDefaultAsync(cancellationToken);
            var profile = await db.AccountantProfiles.AsNoTracking().Where(p => p.OwnerAccountId == peerId)
                .Select(p => new { p.Id, p.DisplayName }).FirstOrDefaultAsync(cancellationToken);
            AccountantId? listedProfile = profile is not null
                && await db.AccountantProfiles.Where(p => p.Id == profile.Id).Where(ListingRules.IsListed).AnyAsync(cancellationToken)
                ? profile.Id
                : null;

            var messages = await db.Messages.AsNoTracking()
                .Where(m => m.ConversationId == conversationId)
                .OrderByDescending(m => m.SentAt).ThenByDescending(m => m.Id)
                .Take(limit)
                .ToListAsync(cancellationToken);
            messages.Reverse();

            return new ThreadData(conversation, PeerName(profile?.DisplayName, peer?.DisplayName), peer?.PictureObject, listedProfile, messages,
                PeerDeleted: peer?.DeletedAt is not null);
        }, cancellationToken);

    public async Task<StoredMessage> AddMessageAsync(
        ChatMessage message, (Guid ClientAccountId, Guid AccountantAccountId)? newPair, CancellationToken cancellationToken)
    {
        // A second attempt covers two first messages to the same accountant at once: the other one created the pair.
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
                // The conversation row is locked until the message is stored, so a block set at the same moment (R15) either
                // comes after this message or makes this send refused; never a message after the block.
                await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
                Conversation conversation;
                if (newPair is { } pair)
                {
                    conversation = (await db.Conversations
                                       .FromSql($"SELECT * FROM conversations WHERE client_account_id = {pair.ClientAccountId} AND accountant_account_id = {pair.AccountantAccountId} FOR UPDATE")
                                       .ToListAsync(cancellationToken)).FirstOrDefault()
                                   ?? db.Conversations.Add(new Conversation(Guid.NewGuid(), pair.ClientAccountId, pair.AccountantAccountId, message.SentAt)).Entity;
                }
                else
                {
                    conversation = (await db.Conversations
                                       .FromSql($"SELECT * FROM conversations WHERE id = {message.ConversationId} FOR UPDATE")
                                       .ToListAsync(cancellationToken)).FirstOrDefault()
                                   ?? throw new InvalidOperationException("The conversation doesn't exist.");
                }

                var stored = new ChatMessage(message.Id, conversation.Id, message.SenderAccountId, message.Text, message.SentAt);
                if (conversation.IsBlocked)
                {
                    return new StoredMessage(StoreMessageOutcome.Blocked, stored, conversation);
                }

                db.Messages.Add(stored);
                conversation.RecordMessage(message.SenderAccountId, message.SentAt);

                // F14 (R08, D18): the other person's notification email, in this same transaction (SDD email contract),
                // only when no email was queued since they last opened the conversation.
                var recipient = conversation.OtherThan(message.SenderAccountId);
                var latestJob = await db.EmailJobs
                    .Where(j => j.RecipientAccountId == recipient && j.ConversationId == conversation.Id)
                    .MaxAsync(j => (DateTimeOffset?)j.CreatedAt, cancellationToken);
                if (EmailRules.NeedsNewJob(conversation.ReadAtFor(recipient), latestJob))
                {
                    db.EmailJobs.Add(new EmailJob(Guid.NewGuid(), recipient, conversation.Id, message.SentAt));
                }

                await db.SaveChangesAsync(cancellationToken); // One statement batch: the conversation, the message and its email.
                await transaction.CommitAsync(cancellationToken);
                return new StoredMessage(StoreMessageOutcome.Stored, stored, conversation);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                if (await FindMessageAsync(message.Id, cancellationToken) is { } earlier)
                {
                    var conversation = await FindAsync(earlier.ConversationId, cancellationToken)
                                       ?? throw Unavailable(ex);
                    return new StoredMessage(StoreMessageOutcome.AlreadyStored, earlier, conversation);
                }

                if (attempt >= 2)
                {
                    throw Unavailable(ex);
                }
            }
            catch (Exception ex) when (IsDataFailure(ex))
            {
                throw Unavailable(ex);
            }
        }
    }

    public Task<Conversation?> SetBlockAsync(Guid conversationId, Guid accountId, bool blocked, DateTimeOffset at, CancellationToken cancellationToken) =>
        Run(async db =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            var conversation = (await db.Conversations
                .FromSql($"SELECT * FROM conversations WHERE id = {conversationId} FOR UPDATE")
                .ToListAsync(cancellationToken)).FirstOrDefault();
            if (conversation is null || !conversation.Includes(accountId))
            {
                return null;
            }

            if (blocked)
            {
                conversation.Block(accountId, at);
            }
            else
            {
                conversation.Unblock(accountId);
            }

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return conversation;
        }, cancellationToken);

    public Task<int> CountSentSinceAsync(Guid senderAccountId, DateTimeOffset since, CancellationToken cancellationToken) =>
        Run(db => db.Messages.CountAsync(m => m.SenderAccountId == senderAccountId && m.SentAt > since, cancellationToken), cancellationToken);

    public Task MarkReadAsync(Guid conversationId, Guid readerId, DateTimeOffset upTo, CancellationToken cancellationToken) =>
        Run(async db =>
        {
            // Only forward, and only the reader's own column (the other side's read time is never touched).
            await db.Conversations
                .Where(c => c.Id == conversationId && c.ClientAccountId == readerId && (c.ClientReadAt == null || c.ClientReadAt < upTo))
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.ClientReadAt, upTo), cancellationToken);
            await db.Conversations
                .Where(c => c.Id == conversationId && c.AccountantAccountId == readerId && (c.AccountantReadAt == null || c.AccountantReadAt < upTo))
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.AccountantReadAt, upTo), cancellationToken);
            return true;
        }, cancellationToken);

    public Task<int> UnreadTotalAsync(Guid accountId, CancellationToken cancellationToken) =>
        Run(db => db.Messages.CountAsync(m => m.SenderAccountId != accountId && db.Conversations.Any(c => c.Id == m.ConversationId
            && ((c.ClientAccountId == accountId && (c.ClientReadAt == null || m.SentAt > c.ClientReadAt))
                || (c.AccountantAccountId == accountId && (c.AccountantReadAt == null || m.SentAt > c.AccountantReadAt)))),
            cancellationToken), cancellationToken);

    /// <summary>An accountant's public name from their profile; otherwise the account's name from sign-in.</summary>
    private static string PeerName(string? profileName, string? accountName) =>
        !string.IsNullOrWhiteSpace(profileName) ? profileName
        : !string.IsNullOrWhiteSpace(accountName) ? accountName
        : "Tax Buddy user";

    private async Task<T> Run<T>(Func<TaxBuddyDbContext, Task<T>> work, CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
            return await work(db);
        }
        catch (Exception ex) when (IsDataFailure(ex))
        {
            throw Unavailable(ex);
        }
    }

    private static bool IsDataFailure(Exception ex) => ex switch
    {
        DbException or TimeoutException or RetryLimitExceededException or DbUpdateException => true,
        InvalidOperationException { InnerException: DbException or TimeoutException } => true,
        _ => false,
    };

    private DataUnavailableException Unavailable(Exception ex)
    {
        // SDD diagnostics: event ID and error category only; never message text, names or IDs.
        logger.LogWarning(DataUnavailable, "Chat data unavailable ({ErrorType})", ex.GetType().Name);
        return new DataUnavailableException(ex);
    }
}
