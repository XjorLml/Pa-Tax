using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using TaxBuddy.Web.Application.Directory;
using TaxBuddy.Web.Application.Reviews;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Data;

/// <summary>Reviews on PostgreSQL (F09). One per client and accountant, enforced by a unique index (SDD ratings).</summary>
public sealed class ReviewStore(IDbContextFactory<TaxBuddyDbContext> contextFactory, ILogger<ReviewStore> logger) : IReviewStore
{
    private static readonly EventId DataUnavailable = new(1005, "ReviewDataUnavailable");

    public Task<ReviewTarget?> FindListedAsync(AccountantId profileId, CancellationToken cancellationToken) =>
        Run(db => db.AccountantProfiles.AsNoTracking()
            .Where(p => p.Id == profileId)
            .Where(ListingRules.IsListed)
            .Select(p => new ReviewTarget(p.Id, p.OwnerAccountId, p.DisplayName))
            .FirstOrDefaultAsync(cancellationToken), cancellationToken);

    public Task<(bool Replied, bool BlockedByAccountant)> ConversationStateAsync(
        Guid clientAccountId, Guid accountantAccountId, CancellationToken cancellationToken) =>
        Run(async db =>
        {
            var state = await db.Conversations.AsNoTracking()
                .Where(c => c.ClientAccountId == clientAccountId && c.AccountantAccountId == accountantAccountId)
                .Select(c => new
                {
                    Replied = db.Messages.Any(m => m.ConversationId == c.Id && m.SenderAccountId == accountantAccountId),
                    Blocked = c.AccountantBlockedAt != null,
                })
                .FirstOrDefaultAsync(cancellationToken);
            return (state?.Replied ?? false, state?.Blocked ?? false);
        }, cancellationToken);

    public Task<Review?> FindAsync(AccountantId profileId, Guid clientAccountId, CancellationToken cancellationToken) =>
        Run(db => db.Reviews.AsNoTracking()
            .FirstOrDefaultAsync(r => r.AccountantId == profileId && r.ClientAccountId == clientAccountId, cancellationToken), cancellationToken);

    public Task<string?> AccountNameAsync(Guid accountId, CancellationToken cancellationToken) =>
        Run(db => db.Users.AsNoTracking().Where(u => u.Id == accountId).Select(u => u.DisplayName).FirstOrDefaultAsync(cancellationToken),
            cancellationToken);

    public Task<bool> SaveAsync(AccountantId profileId, Guid clientAccountId, int stars, string? text, DateTimeOffset at, CancellationToken cancellationToken) =>
        Run(async db =>
        {
            for (var attempt = 0; ; attempt++)
            {
                var review = await db.Reviews
                    .FirstOrDefaultAsync(r => r.AccountantId == profileId && r.ClientAccountId == clientAccountId, cancellationToken);
                if (review is not null)
                {
                    review.Change(stars, text, at);
                    await db.SaveChangesAsync(cancellationToken);
                    return true;
                }

                db.Reviews.Add(new Review(Guid.NewGuid(), profileId, clientAccountId, stars, text, at));
                try
                {
                    await db.SaveChangesAsync(cancellationToken);
                    return false;
                }
                catch (DbUpdateException ex) when (attempt == 0 && ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
                {
                    // The same client posted twice at once (two tabs, a double press): the other post won; change that one.
                    db.ChangeTracker.Clear();
                }
            }
        }, cancellationToken);

    public Task<bool> DeleteAsync(AccountantId profileId, Guid clientAccountId, CancellationToken cancellationToken) =>
        Run(async db => await db.Reviews
            .Where(r => r.AccountantId == profileId && r.ClientAccountId == clientAccountId)
            .ExecuteDeleteAsync(cancellationToken) > 0, cancellationToken);

    public Task<bool> SaveReplyAsync(AccountantId profileId, Guid reviewId, string text, DateTimeOffset at, CancellationToken cancellationToken) =>
        Run(async db =>
        {
            var review = await db.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId && r.AccountantId == profileId, cancellationToken);
            if (review is null)
            {
                return false;
            }

            review.SetReply(text, at);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }, cancellationToken);

    public Task<IReadOnlyList<ReviewRow>> ListAsync(AccountantId profileId, CancellationToken cancellationToken) =>
        Run<IReadOnlyList<ReviewRow>>(async db => await db.Reviews.AsNoTracking()
            .Where(r => r.AccountantId == profileId)
            .OrderByDescending(r => r.UpdatedAt)
            .ThenBy(r => r.Id)
            .Select(r => new ReviewRow(
                r.Id,
                r.ClientAccountId,
                db.Users.Where(u => u.Id == r.ClientAccountId).Select(u => u.DisplayName).FirstOrDefault(),
                r.Stars,
                r.Text,
                r.UpdatedAt,
                r.ReplyText,
                r.ReplyAt))
            .ToListAsync(cancellationToken), cancellationToken);

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
        // SDD diagnostics: event ID and error category only; never review text, names or IDs.
        logger.LogWarning(DataUnavailable, "Review data unavailable ({ErrorType})", ex.GetType().Name);
        return new DataUnavailableException(ex);
    }
}
