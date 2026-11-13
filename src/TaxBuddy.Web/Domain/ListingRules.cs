using System.Linq.Expressions;

namespace TaxBuddy.Web.Domain;

/// <summary>
/// PRD BR01: a profile is listed only while its required fields are complete. There is no approval step.
/// Required fields per D04 (provisional): name, title, summary, email, at least one service and consent to show
/// contact details. The same expression filters database queries and in-memory checks, so the rule has one source.
/// Account deletion (R09) removes the profile row itself, so a deleted account's profile cannot be listed.
/// </summary>
public static class ListingRules
{
    public static Expression<Func<AccountantProfile, bool>> IsListed { get; } = p =>
        p.DisplayName.Trim() != ""
        && p.Title.Trim() != ""
        && p.Summary.Trim() != ""
        && p.Email.Trim() != ""
        && p.ConsentToShowContact
        && p.Services.Any();

    private static readonly Func<AccountantProfile, bool> IsListedCompiled = IsListed.Compile();

    public static bool IsListedProfile(AccountantProfile profile) => IsListedCompiled(profile);

    /// <summary>What is still missing before the profile can be listed, in display order.</summary>
    public static IReadOnlyList<string> MissingForListing(AccountantProfile profile)
    {
        var missing = new List<string>();
        if (profile.DisplayName.Trim() == "") missing.Add("display name");
        if (profile.Title.Trim() == "") missing.Add("professional title");
        if (profile.Summary.Trim() == "") missing.Add("career summary");
        if (profile.Services.Count == 0) missing.Add("at least one service");
        if (profile.Email.Trim() == "") missing.Add("email");
        if (!profile.ConsentToShowContact) missing.Add("agreement to show contact details");
        return missing;
    }
}

/// <summary>Years of experience, counted from the earliest start month (PRD D04, provisional).</summary>
public static class ExperienceYears
{
    /// <summary>Whole years from the earliest start month to <paramref name="today"/>; null when there is no experience or less than a year.</summary>
    public static int? Calculate(IEnumerable<YearMonth> startMonths, DateOnly today)
    {
        var starts = startMonths.ToList();
        if (starts.Count == 0)
        {
            return null;
        }

        var earliest = starts.Min();
        var years = earliest.MonthsUntil(YearMonth.From(today)) / 12;
        return years >= 1 ? years : null;
    }
}
