using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TaxBuddy.Web.Application.Directory;
using TaxBuddy.Web.Application.Pictures;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Data;

/// <summary>Directory queries on PostgreSQL (SDD: queries; PRD BR01, BR02, D05; ratings R13, F09).</summary>
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

            // BR02: name A–Z with the unique ID as tie-breaker, so paging never repeats or skips anyone. "Highest rated"
            // (F09) puts reviewed accountants first, by average and then count; the average is computed, never stored.
            var ordered = query.Sort == DirectorySort.Rating
                ? matching
                    .OrderByDescending(p => db.Reviews.Any(r => r.AccountantId == p.Id))
                    .ThenByDescending(p => db.Reviews.Where(r => r.AccountantId == p.Id).Average(r => (double?)r.Stars))
                    .ThenByDescending(p => db.Reviews.Count(r => r.AccountantId == p.Id))
                    .ThenBy(p => p.DisplayName.ToLower())
                    .ThenBy(p => p.Id)
                : matching
                    .OrderBy(p => p.DisplayName.ToLower())
                    .ThenBy(p => p.Id);

            var rows = await CardRowsAsync(db, ordered.Skip((page - 1) * pageSize).Take(pageSize), cancellationToken);
            var anyListed = matchCount > 0 || (query.IsFiltered && await listed.AnyAsync(cancellationToken));

            return new DirectoryRows(rows, matchCount, page, anyListed);
        }
        catch (Exception ex) when (IsDataFailure(ex))
        {
            throw Unavailable(ex);
        }
    }

    public async Task<CardRow?> FindCardAsync(AccountantId id, CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
            var listed = db.AccountantProfiles.AsNoTracking().Where(ListingRules.IsListed).Where(p => p.Id == id);
            return (await CardRowsAsync(db, listed, cancellationToken)).SingleOrDefault();
        }
        catch (Exception ex) when (IsDataFailure(ex))
        {
            throw Unavailable(ex);
        }
    }

    /// <summary>
    /// The card rows for <paramref name="profiles"/>, in their order: the card's fields, the review totals (F09) and, in one
    /// more query, the owners' pictures (F06). Directory pages and the editor's preview read cards the same way.
    /// </summary>
    private static async Task<List<CardRow>> CardRowsAsync(
        TaxBuddyDbContext db, IQueryable<AccountantProfile> profiles, CancellationToken cancellationToken)
    {
        var rows = await profiles
            .Select(p => new
            {
                p.Id,
                p.DisplayName,
                p.Title,
                p.City,
                p.Summary,
                p.OwnerAccountId,
                Services = p.Services.OrderBy(s => s.Position).Select(s => s.Name).ToList(),
                Starts = p.Experience.Select(e => e.Start).ToList(),
                Highlights = p.Highlights.Count(),
                ReviewCount = db.Reviews.Count(r => r.AccountantId == p.Id),
                StarTotal = db.Reviews.Where(r => r.AccountantId == p.Id).Sum(r => r.Stars),
            })
            .ToListAsync(cancellationToken);

        var owners = rows.Where(r => r.OwnerAccountId is not null).Select(r => r.OwnerAccountId!.Value).ToList();
        var pictures = owners.Count == 0
            ? new Dictionary<Guid, string?>()
            : await db.Users.AsNoTracking()
                .Where(u => owners.Contains(u.Id) && u.PictureObject != null)
                .ToDictionaryAsync(u => u.Id, u => u.PictureObject, cancellationToken);

        return rows.Select(r => new CardRow(r.Id, r.DisplayName, r.Title, r.City, r.Summary, r.Services, r.Starts,
            r.OwnerAccountId is { } owner && pictures.TryGetValue(owner, out var picture)
                ? ProfilePictureService.AddressFor(owner, picture)
                : null,
            r.Highlights,
            r.ReviewCount,
            r.StarTotal)).ToList();
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

    public async Task<string?> PictureAddressAsync(AccountantProfile profile, CancellationToken cancellationToken)
    {
        if (profile.OwnerAccountId is not { } owner)
        {
            return null;
        }

        try
        {
            await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
            var picture = await db.Users.AsNoTracking().Where(u => u.Id == owner).Select(u => u.PictureObject).FirstOrDefaultAsync(cancellationToken);
            return ProfilePictureService.AddressFor(owner, picture);
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
