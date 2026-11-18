using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using TaxBuddy.Web.Application.Directory;
using TaxBuddy.Web.Application.Reports;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Data;

/// <summary>Reports on PostgreSQL (F10). One per reporter and item, enforced by a unique index. The owner reads the table (D22).</summary>
public sealed class ReportStore(IDbContextFactory<TaxBuddyDbContext> contextFactory, ILogger<ReportStore> logger) : IReportStore
{
    private static readonly EventId DataUnavailable = new(1006, "ReportDataUnavailable");

    public Task<ReportSubject?> FindAsync(ReportKind kind, Guid targetId, CancellationToken cancellationToken) =>
        Run(async db =>
        {
            switch (kind)
            {
                case ReportKind.Profile:
                    var profileId = new AccountantId(targetId);
                    return await db.AccountantProfiles.AsNoTracking()
                        .Where(p => p.Id == profileId)
                        .Where(ListingRules.IsListed)
                        .Select(p => new ReportSubject(p.Id, p.DisplayName, p.OwnerAccountId, null, p.OwnerAccountId, null))
                        .FirstOrDefaultAsync(cancellationToken);
                case ReportKind.Review:
                    var review = await db.Reviews.AsNoTracking()
                        .Where(r => r.Id == targetId)
                        .Select(r => new
                        {
                            r.AccountantId,
                            r.ClientAccountId,
                            ClientName = db.Users.Where(u => u.Id == r.ClientAccountId).Select(u => u.DisplayName).FirstOrDefault(),
                        })
                        .FirstOrDefaultAsync(cancellationToken);
                    if (review is null)
                    {
                        return null;
                    }

                    var listed = await db.AccountantProfiles.AsNoTracking()
                        .Where(p => p.Id == review.AccountantId)
                        .Where(ListingRules.IsListed)
                        .Select(p => new { p.DisplayName, p.OwnerAccountId })
                        .FirstOrDefaultAsync(cancellationToken);
                    return listed is null
                        ? null
                        : new ReportSubject(review.AccountantId, listed.DisplayName, review.ClientAccountId, review.ClientAccountId,
                            listed.OwnerAccountId, review.ClientName);
                default:
                    var conversation = await db.Conversations.AsNoTracking()
                        .Where(c => c.Id == targetId)
                        .Select(c => new
                        {
                            c.ClientAccountId,
                            c.AccountantAccountId,
                            ClientName = db.Users.Where(u => u.Id == c.ClientAccountId).Select(u => u.DisplayName).FirstOrDefault(),
                            AccountName = db.Users.Where(u => u.Id == c.AccountantAccountId).Select(u => u.DisplayName).FirstOrDefault(),
                            Profile = db.AccountantProfiles.Where(p => p.OwnerAccountId == c.AccountantAccountId)
                                .Select(p => new { p.Id, p.DisplayName }).FirstOrDefault(),
                        })
                        .FirstOrDefaultAsync(cancellationToken);
                    return conversation is null
                        ? null
                        : new ReportSubject(conversation.Profile?.Id, conversation.Profile?.DisplayName ?? conversation.AccountName ?? "an accountant",
                            null, conversation.ClientAccountId, conversation.AccountantAccountId, conversation.ClientName);
            }
        }, cancellationToken);

    public Task<bool> ExistsAsync(Guid reporterAccountId, ReportKind kind, Guid targetId, CancellationToken cancellationToken) =>
        Run(db => db.Reports.AnyAsync(r => r.ReporterAccountId == reporterAccountId && r.Kind == kind && r.TargetId == targetId, cancellationToken),
            cancellationToken);

    public Task<int> CountSinceAsync(Guid reporterAccountId, DateTimeOffset since, CancellationToken cancellationToken) =>
        Run(db => db.Reports.CountAsync(r => r.ReporterAccountId == reporterAccountId && r.CreatedAt > since, cancellationToken), cancellationToken);

    public async Task<bool> AddAsync(Report report, CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
            db.Reports.Add(report);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return false; // Sent twice at once: the first one is stored.
        }
        catch (Exception ex) when (IsDataFailure(ex))
        {
            throw Unavailable(ex);
        }
    }

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
        // SDD diagnostics: event ID and error category only; never reasons, details, names or IDs.
        logger.LogWarning(DataUnavailable, "Report data unavailable ({ErrorType})", ex.GetType().Name);
        return new DataUnavailableException(ex);
    }
}
