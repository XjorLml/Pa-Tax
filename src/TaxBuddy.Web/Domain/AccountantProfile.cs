namespace TaxBuddy.Web.Domain;

/// <summary>
/// Provisional field length limits. PRD D04 leaves limits open; these are working values chosen at
/// implementation so the database never holds unbounded text (SDD: abuse limits). Revisit with R05.
/// </summary>
public static class ProfileLimits
{
    public const int DisplayName = 100;
    public const int Title = 100;
    public const int City = 80;
    public const int Summary = 1000;
    public const int Email = 254;
    public const int Phone = 30;
    public const int Website = 200;
    public const int ServiceDescription = 200;
    public const int ExperienceText = 100;
}

/// <summary>An accountant's public profile (PRD R03, R05; fields per D04, adopted provisionally).</summary>
public sealed class AccountantProfile
{
    private readonly List<OfferedService> _services = [];
    private readonly List<ExperienceEntry> _experience = [];

    private AccountantProfile()
    {
        // For EF Core.
    }

    public AccountantProfile(
        AccountantId id,
        string displayName,
        string title,
        string? city,
        string summary,
        string email,
        string? phone,
        string? website,
        bool consentToShowContact)
    {
        Id = id;
        DisplayName = Text(displayName, ProfileLimits.DisplayName, nameof(displayName));
        Title = Text(title, ProfileLimits.Title, nameof(title));
        City = Optional(city, ProfileLimits.City, nameof(city));
        Summary = Text(summary, ProfileLimits.Summary, nameof(summary));
        Email = Text(email, ProfileLimits.Email, nameof(email));
        Phone = Optional(phone, ProfileLimits.Phone, nameof(phone));
        Website = Optional(website, ProfileLimits.Website, nameof(website));
        ConsentToShowContact = consentToShowContact;
    }

    public AccountantId Id { get; private set; }

    public string DisplayName { get; private set; } = "";

    public string Title { get; private set; } = "";

    public string? City { get; private set; }

    public string Summary { get; private set; } = "";

    public string Email { get; private set; } = "";

    public string? Phone { get; private set; }

    public string? Website { get; private set; }

    /// <summary>The accountant agreed to show contact details publicly (PRD R11, D04).</summary>
    public bool ConsentToShowContact { get; private set; }

    public IReadOnlyList<OfferedService> Services => _services;

    public IReadOnlyList<ExperienceEntry> Experience => _experience;

    /// <summary>Whether the profile appears in the directory (PRD BR01).</summary>
    public bool IsListed => ListingRules.IsListedProfile(this);

    public void AddService(string name, string description)
    {
        var catalogName = ServiceCatalog.Find(name)
            ?? throw new ArgumentException($"'{name}' is not a service in the catalog.", nameof(name));
        if (_services.Any(s => s.Name == catalogName))
        {
            throw new ArgumentException($"'{catalogName}' is already listed.", nameof(name));
        }

        _services.Add(new OfferedService(catalogName, Optional(description, ProfileLimits.ServiceDescription, nameof(description)) ?? "", _services.Count));
    }

    public void AddExperience(string role, string organization, YearMonth start, YearMonth? end)
    {
        if (end is { } e && e.CompareTo(start) < 0)
        {
            throw new ArgumentException("An experience entry cannot end before it starts.", nameof(end));
        }

        _experience.Add(new ExperienceEntry(
            Text(role, ProfileLimits.ExperienceText, nameof(role)),
            Text(organization, ProfileLimits.ExperienceText, nameof(organization)),
            start,
            end));
    }

    // Text may be empty: an unfinished profile is saved but not listed (BR01).
    private static string Text(string value, int max, string name)
    {
        var trimmed = (value ?? "").Trim();
        ArgumentOutOfRangeException.ThrowIfGreaterThan(trimmed.Length, max, name);
        return trimmed;
    }

    private static string? Optional(string? value, int max, string name)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        ArgumentOutOfRangeException.ThrowIfGreaterThan(trimmed.Length, max, name);
        return trimmed;
    }
}

/// <summary>A service from <see cref="ServiceCatalog"/> with the accountant's own short description.</summary>
public sealed class OfferedService
{
    private OfferedService()
    {
        // For EF Core.
    }

    internal OfferedService(string name, string description, int position)
    {
        Name = name;
        Description = description;
        Position = position;
    }

    public string Name { get; private set; } = "";

    public string Description { get; private set; } = "";

    /// <summary>Display order chosen by the accountant.</summary>
    public int Position { get; private set; }
}

/// <summary>One role in the accountant's experience. A null end means the role is current.</summary>
public sealed class ExperienceEntry
{
    private ExperienceEntry()
    {
        // For EF Core.
    }

    internal ExperienceEntry(string role, string organization, YearMonth start, YearMonth? end)
    {
        Role = role;
        Organization = organization;
        Start = start;
        End = end;
    }

    public string Role { get; private set; } = "";

    public string Organization { get; private set; } = "";

    public YearMonth Start { get; private set; }

    public YearMonth? End { get; private set; }

    public bool IsCurrent => End is null;
}
