using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Application.Directory;

/// <summary>One page of the directory, with the state the page must show (DESIGN: ready, empty directory, no match).</summary>
public sealed record DirectoryResult(
    DirectoryQuery Query,
    IReadOnlyList<AccountantCard> Cards,
    int MatchCount,
    int PageCount,
    bool DirectoryIsEmpty)
{
    public int Page => Query.Page;

    public bool NoMatch => MatchCount == 0 && !DirectoryIsEmpty;
}

/// <summary>A directory card: name, short career summary and services (PRD R01), plus city and years (DESIGN card order).</summary>
public sealed record AccountantCard(
    AccountantId Id,
    string Name,
    string Title,
    string? City,
    int? YearsOfExperience,
    IReadOnlyList<string> Services,
    string Summary);

/// <summary>Everything the public profile shows (PRD R03). Contact details are public by the accountant's consent (R11).</summary>
public sealed record AccountantProfileView(
    AccountantId Id,
    string Name,
    string Title,
    string? City,
    int? YearsOfExperience,
    string Summary,
    IReadOnlyList<ExperienceView> Experience,
    IReadOnlyList<ServiceView> Services,
    string Email,
    string? Phone,
    string? Website,
    string? WebsiteHref)
{
    public static AccountantProfileView From(AccountantProfile profile, DateOnly today) => new(
        profile.Id,
        profile.DisplayName,
        profile.Title,
        profile.City,
        ExperienceYears.Calculate(profile.Experience.Select(e => e.Start), today),
        profile.Summary,
        profile.Experience
            .OrderByDescending(e => e.End is null)
            .ThenByDescending(e => e.Start)
            .Select(e => new ExperienceView(e.Role, e.Organization, e.Start.ToDisplayString(), e.End?.ToDisplayString()))
            .ToList(),
        profile.Services.OrderBy(s => s.Position).Select(s => new ServiceView(s.Name, s.Description)).ToList(),
        profile.Email,
        profile.Phone,
        profile.Website,
        WebsiteLink(profile.Website));

    /// <summary>Only an http(s) address with a host becomes a link; anything else is shown as text.</summary>
    private static string? WebsiteLink(string? website)
    {
        if (string.IsNullOrWhiteSpace(website))
        {
            return null;
        }

        var candidate = website.Contains("://", StringComparison.Ordinal) ? website : "https://" + website;
        return Uri.TryCreate(candidate, UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
               && uri.Host.Contains('.', StringComparison.Ordinal)
            ? uri.AbsoluteUri
            : null;
    }
}

/// <summary>An experience row. A null <see cref="Until"/> means the role is current.</summary>
public sealed record ExperienceView(string Role, string Organization, string From, string? Until);

public sealed record ServiceView(string Name, string Description);

/// <summary>Rows read by the data store for one directory page.</summary>
public sealed record DirectoryRows(IReadOnlyList<CardRow> Rows, int MatchCount, int Page, bool AnyListed);

public sealed record CardRow(
    AccountantId Id,
    string Name,
    string Title,
    string? City,
    string Summary,
    IReadOnlyList<string> Services,
    IReadOnlyList<YearMonth> ExperienceStarts);

/// <summary>Reads listed profiles. Implemented in Data; the Application layer never sees EF Core types.</summary>
public interface IDirectoryStore
{
    /// <summary>
    /// Listed profiles matching <paramref name="query"/>, ordered by name A–Z then ID (PRD BR02).
    /// A page past the end falls back to page 1, as malformed address values are ignored.
    /// </summary>
    Task<DirectoryRows> SearchAsync(DirectoryQuery query, int pageSize, CancellationToken cancellationToken);

    /// <summary>The profile when it exists and is listed (PRD BR01); otherwise null.</summary>
    Task<AccountantProfile?> FindListedAsync(AccountantId id, CancellationToken cancellationToken);
}

/// <summary>The database could not be reached or failed. Pages show an error with a retry, never an empty list (R01, R10).</summary>
public sealed class DataUnavailableException(Exception innerException)
    : Exception("Directory data is unavailable.", innerException);
