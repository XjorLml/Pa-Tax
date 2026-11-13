using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Application.Directory;

/// <summary>
/// Public directory and profile reads (PRD R01–R03, F01). Reads are open to everyone and return only listed profiles (BR01).
/// </summary>
public sealed class DirectoryService(IDirectoryStore store, IClock clock)
{
    /// <summary>
    /// Working page size. PRD D05 leaves the page size open; 6 matches the reviewed prototype. Change it here only.
    /// </summary>
    public const int PageSize = 6;

    /// <exception cref="DataUnavailableException">The database could not be read.</exception>
    public async Task<DirectoryResult> SearchAsync(DirectoryQuery query, CancellationToken cancellationToken = default)
    {
        var rows = await store.SearchAsync(query, PageSize, cancellationToken);
        var today = ManilaTime.Today(clock);
        var cards = rows.Rows
            .Select(r => new AccountantCard(
                r.Id, r.Name, r.Title, r.City, ExperienceYears.Calculate(r.ExperienceStarts, today), r.Services, r.Summary))
            .ToList();
        var pageCount = Math.Max(1, (rows.MatchCount + PageSize - 1) / PageSize);

        return new DirectoryResult(
            query with { Page = rows.Page },
            cards,
            rows.MatchCount,
            pageCount,
            DirectoryIsEmpty: rows.MatchCount == 0 && !rows.AnyListed);
    }

    /// <summary>The listed profile for an address value; null for a malformed, unknown, deleted or incomplete profile (FT05).</summary>
    /// <exception cref="DataUnavailableException">The database could not be read.</exception>
    public async Task<AccountantProfileView?> GetProfileAsync(string? id, CancellationToken cancellationToken = default)
    {
        if (!AccountantId.TryParse(id, out var accountantId))
        {
            return null;
        }

        var profile = await store.FindListedAsync(accountantId, cancellationToken);
        return profile is null ? null : AccountantProfileView.From(profile, ManilaTime.Today(clock));
    }
}
