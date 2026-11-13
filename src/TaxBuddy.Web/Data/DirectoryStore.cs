using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TaxBuddy.Web.Application.Directory;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Data;

/// <summary>Directory queries on PostgreSQL (SDD: queries; PRD BR01, BR02, D05).</summary>
public sealed class DirectoryStore(IDbContextFactory<TaxBuddyDbContext> contextFactory, ILogger<DirectoryStore> logger)
    : IDirectoryStore
{
    private const string LikeEscape = "\\";
    private static readonly EventId DataUnavailable = new(1001, "DirectoryDataUnavailable");

    public async Task<DirectoryRows> SearchAsync(DirectoryQuery query, int pageSize, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        try
        {
            await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
            var listed = db.AccountantProfiles.AsNoTracking().Where(ListingRules.IsListed);

            var matching = listed;
            if (query.Service is { } service)
            {
                matching = matching.Where(p => p.Services.Any(s => s.Name == service));
            }

            if (query.SearchText is { } text)
            {
                // D05: text search on name, title, city and service. User text is literal, never a LIKE pattern.
                var pattern = "%" + EscapeLike(text) + "%";
                matching = matching.Where(p =>
                    EF.Functions.ILike(p.DisplayName, pattern, LikeEscape)
                    || EF.Functions.ILike(p.Title, pattern, LikeEscape)
                    || (p.City != null && EF.Functions.ILike(p.City, pattern, LikeEscape))
                    || p.Services.Any(s => EF.Functions.ILike(s.Name, pattern, LikeEscape)));
            }

            var matchCount = await matching.CountAsync(cancellationToken);
            var pageCount = Math.Max(1, (matchCount + pageSize - 1) / pageSize);
            var page = query.Page <= pageCount ? query.Page : 1;

            // BR02: name A–Z with the unique ID as tie-breaker, so paging never repeats or skips anyone.
            var rows = await matching
                .OrderBy(p => p.DisplayName.ToLower())
                .ThenBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    p.DisplayName,
                    p.Title,
                    p.City,
                    p.Summary,
                    Services = p.Services.OrderBy(s => s.Position).Select(s => s.Name).ToList(),
                    Starts = p.Experience.Select(e => e.Start).ToList(),
                })
                .ToListAsync(cancellationToken);

            var anyListed = matchCount > 0 || (query.IsFiltered && await listed.AnyAsync(cancellationToken));

            return new DirectoryRows(
                rows.Select(r => new CardRow(r.Id, r.DisplayName, r.Title, r.City, r.Summary, r.Services, r.Starts)).ToList(),
                matchCount,
                page,
                anyListed);
        }
        catch (Exception ex) when (IsDataFailure(ex))
        {
            throw Unavailable(ex);
        }
    }

    public async Task<AccountantProfile?> FindListedAsync(AccountantId id, CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
            return await db.AccountantProfiles
                .AsNoTracking()
                .Where(ListingRules.IsListed)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }
        catch (Exception ex) when (IsDataFailure(ex))
        {
            throw Unavailable(ex);
        }
    }

    internal static string EscapeLike(string text) =>
        text.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);

    private static bool IsDataFailure(Exception ex) => ex switch
    {
        DbException or TimeoutException or RetryLimitExceededException => true,
        InvalidOperationException { InnerException: DbException or TimeoutException } => true,
        _ => false,
    };

    private DataUnavailableException Unavailable(Exception ex)
    {
        // SDD diagnostics: event ID and error category only; exception messages can carry host names.
        logger.LogWarning(DataUnavailable, "Directory data unavailable ({ErrorType})", ex.GetType().Name);
        return new DataUnavailableException(ex);
    }
}
