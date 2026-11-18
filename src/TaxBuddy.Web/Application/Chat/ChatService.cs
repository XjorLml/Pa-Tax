using TaxBuddy.Web.Application.Directory;
using TaxBuddy.Web.Application.Pictures;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Application.Chat;

/// <summary>
/// Chat between clients and accountants (PRD R06, R07, BR03–BR05; F07). Every read and write checks that the account
/// takes part in the conversation; an ID in an address never proves access (BR05). Data failures surface as
/// <see cref="DataUnavailableException"/>, except when sending, which reports <see cref="SendOutcome.Failed"/>.
/// </summary>
public sealed class ChatService(IChatStore store, IChatNotifier notifier, IClock clock)
{
    /// <summary>Where a "Message" action leads (CH01, CH02, CH08).</summary>
    public async Task<NewChat> PrepareNewChatAsync(ChatViewer? viewer, AccountantId profileId, CancellationToken cancellationToken = default)
    {
        if (await store.FindListedAccountantAsync(profileId, cancellationToken) is not { } listed)
        {
            return new NewChat(NewChatOutcome.Unavailable);
        }

        if (listed.OwnerAccountId is not { } accountantAccountId)
        {
            return new NewChat(NewChatOutcome.NoAccount);
        }

        var recipient = new ChatRecipient(listed.ProfileId, accountantAccountId, listed.Name,
            ProfilePictureService.AddressFor(accountantAccountId, listed.PictureObject));
        if (viewer is null)
        {
            return new NewChat(NewChatOutcome.Ready, recipient); // A visitor may write first (R06).
        }

        if (!viewer.IsClient)
        {
            return new NewChat(NewChatOutcome.NotAClient, recipient);
        }

        return await store.FindConversationIdAsync(viewer.AccountId, accountantAccountId, cancellationToken) is { } existing
            ? new NewChat(NewChatOutcome.Existing, recipient, existing)
            : new NewChat(NewChatOutcome.Ready, recipient);
    }

    public async Task<IReadOnlyList<ConversationSummary>> ListAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var rows = await store.ListAsync(accountId, cancellationToken);
        return rows.Select(r => new ConversationSummary(
                r.Id,
                r.PeerName,
                ProfilePictureService.AddressFor(r.PeerAccountId, r.PeerPictureObject),
                r.LastText,
                r.LastSenderId == accountId,
                r.LastAt,
                r.Unread))
            .ToList();
    }

    /// <summary>The thread, or null when the account isn't part of it or it doesn't exist (CH07). Opening it marks it read.</summary>
    public async Task<ThreadView?> OpenThreadAsync(Guid accountId, Guid conversationId, bool markRead = true, CancellationToken cancellationToken = default)
    {
        if (await store.LoadThreadAsync(conversationId, accountId, ChatLimits.ThreadMessages, cancellationToken) is not { } data
            || !data.Conversation.Includes(accountId))
        {
            return null;
        }

        var lastFromPeer = data.Messages.LastOrDefault(m => m.SenderAccountId != accountId);
        if (markRead && lastFromPeer is not null && data.Conversation.MarkRead(accountId, lastFromPeer.SentAt))
        {
            await store.MarkReadAsync(conversationId, accountId, lastFromPeer.SentAt, cancellationToken);
            notifier.Changed(data.Conversation); // The reader's other tabs and the navigation count.
        }

        var peerId = data.Conversation.OtherThan(accountId);
        return new ThreadView(
            conversationId,
            data.Conversation.ClientAccountId == accountId,
            data.PeerName,
            ProfilePictureService.AddressFor(peerId, data.PeerPictureObject),
            data.PeerListedProfileId,
            data.Messages.Select(m => View(m, accountId)).ToList())
        {
            BlockedByMe = data.Conversation.IsBlockedBy(accountId),
            BlockedByThem = data.Conversation.IsBlockedBy(peerId),
        };
    }

    /// <summary>
    /// Blocks or unblocks the other participant (R15): while either side has blocked, neither can send in the conversation.
    /// Only a participant can, and only their own block. False when the account isn't part of the conversation. Both open
    /// panels update through the notifier; the blocked person is not told why.
    /// </summary>
    /// <exception cref="DataUnavailableException">The database could not be reached.</exception>
    public async Task<bool> SetBlockAsync(Guid accountId, Guid conversationId, bool blocked, CancellationToken cancellationToken = default)
    {
        if (await store.SetBlockAsync(conversationId, accountId, blocked, clock.UtcNow, cancellationToken) is not { } conversation)
        {
            return false;
        }

        notifier.Changed(conversation);
        return true;
    }

    /// <summary>
    /// Sends a message, either in a conversation or, with <paramref name="newChatWith"/>, as a client's first message to an
    /// accountant. Sending the same <paramref name="messageId"/> again returns the stored message (BR04).
    /// </summary>
    public async Task<SendResult> SendAsync(
        ChatViewer viewer, Guid? conversationId, AccountantId? newChatWith, Guid messageId, string? text,
        CancellationToken cancellationToken = default)
    {
        var (normalized, problem) = MessageText.Normalize(text);
        if (normalized is null)
        {
            return new SendResult(SendOutcome.Invalid, Problem: problem);
        }

        try
        {
            if (await store.FindMessageAsync(messageId, cancellationToken) is { } earlier)
            {
                // A repeated send. Only the same sender may claim it; anything else is a forged ID.
                if (earlier.SenderAccountId != viewer.AccountId || (conversationId is { } c && earlier.ConversationId != c))
                {
                    return new SendResult(SendOutcome.NotAllowed);
                }

                return new SendResult(SendOutcome.Sent, earlier.ConversationId, View(earlier, viewer.AccountId));
            }

            (Guid ClientAccountId, Guid AccountantAccountId)? newPair = null;
            Guid targetConversation;
            if (conversationId is { } id)
            {
                if (await store.FindAsync(id, cancellationToken) is not { } conversation || !conversation.Includes(viewer.AccountId))
                {
                    return new SendResult(SendOutcome.NotAllowed);
                }

                if (conversation.IsBlocked)
                {
                    return new SendResult(SendOutcome.Blocked, id);
                }

                targetConversation = id;
            }
            else if (newChatWith is { } profileId)
            {
                var prepared = await PrepareNewChatAsync(viewer, profileId, cancellationToken);
                if (prepared.Recipient is not { } recipient || prepared.Outcome is not (NewChatOutcome.Ready or NewChatOutcome.Existing))
                {
                    return new SendResult(SendOutcome.NotAllowed);
                }

                targetConversation = prepared.ConversationId ?? Guid.Empty; // Empty: the store finds or creates it.
                newPair = (viewer.AccountId, recipient.AccountId);
            }
            else
            {
                return new SendResult(SendOutcome.NotAllowed);
            }

            var now = clock.UtcNow;
            if (await store.CountSentSinceAsync(viewer.AccountId, now.AddMinutes(-1), cancellationToken) >= ChatLimits.MessagesPerMinute)
            {
                return new SendResult(SendOutcome.TooFast);
            }

            var stored = await store.AddMessageAsync(
                new ChatMessage(messageId, targetConversation, viewer.AccountId, normalized, now), newPair, cancellationToken);
            if (stored.Outcome == StoreMessageOutcome.AlreadyStored && stored.Message.SenderAccountId != viewer.AccountId)
            {
                return new SendResult(SendOutcome.NotAllowed);
            }

            if (stored.Outcome == StoreMessageOutcome.Blocked)
            {
                // A new chat to an accountant who already blocked this client lands here, as does a send racing a block.
                return new SendResult(SendOutcome.Blocked, stored.Conversation.Id);
            }

            notifier.Changed(stored.Conversation);
            return new SendResult(SendOutcome.Sent, stored.Conversation.Id, View(stored.Message, viewer.AccountId));
        }
        catch (DataUnavailableException)
        {
            return new SendResult(SendOutcome.Failed);
        }
    }

    /// <summary>After a send with an unknown outcome: whether the message was stored ("Checking whether it was sent…").</summary>
    public async Task<bool?> IsStoredAsync(Guid accountId, Guid messageId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await store.FindMessageAsync(messageId, cancellationToken) is { } message && message.SenderAccountId == accountId;
        }
        catch (DataUnavailableException)
        {
            return null;
        }
    }

    public Task<int> UnreadTotalAsync(Guid accountId, CancellationToken cancellationToken = default) =>
        store.UnreadTotalAsync(accountId, cancellationToken);

    private static MessageView View(ChatMessage message, Guid viewerId) =>
        new(message.Id, message.SenderAccountId == viewerId, message.Text, message.SentAt);
}
