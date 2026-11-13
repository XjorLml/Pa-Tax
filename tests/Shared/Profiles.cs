using TaxBuddy.Web.Domain;

namespace TaxBuddy.TestSupport;

/// <summary>Synthetic profiles for tests. Every name and address is invented.</summary>
public static class Profiles
{
    public static AccountantProfile Listed(
        string name,
        string title = "Certified Public Accountant",
        string? city = "Makati City",
        string[]? services = null,
        Guid? id = null,
        YearMonth? start = null)
    {
        var profile = new AccountantProfile(
            id is { } g ? new AccountantId(g) : AccountantId.New(),
            name,
            title,
            city,
            $"Synthetic summary for {name}.",
            $"{Guid.NewGuid():N}@example.com",
            "+63 900 000 0000",
            null,
            consentToShowContact: true);
        foreach (var service in services ?? ["Bookkeeping"])
        {
            profile.AddService(service, $"{service} for small businesses.");
        }

        profile.AddExperience("Accountant", "Synthetic Firm", start ?? new YearMonth(2015, 1), null);
        return profile;
    }

    /// <summary>Complete except for consent to show contact details, so never listed (BR01).</summary>
    public static AccountantProfile WithoutConsent(string name) =>
        new(AccountantId.New(), name, "Bookkeeper", "Quezon City", "Synthetic summary.", $"{Guid.NewGuid():N}@example.com", null, null, consentToShowContact: false);

    /// <summary>Complete except for services, so never listed (BR01).</summary>
    public static AccountantProfile WithoutServices(string name) =>
        new(AccountantId.New(), name, "Bookkeeper", "Quezon City", "Synthetic summary.", $"{Guid.NewGuid():N}@example.com", null, null, consentToShowContact: true);
}

public sealed class FixedClock(DateTimeOffset now) : IClock
{
    public DateTimeOffset UtcNow => now;
}
