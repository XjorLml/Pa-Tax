using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Application.Profiles;

/// <summary>
/// The profile editor's values exactly as typed (F05). Posted by the static form and shown again unchanged when a
/// save fails, so nothing typed is lost (C04 for this page).
/// </summary>
public sealed class ProfileForm
{
    private List<ServiceInput> services = [];
    private List<ExperienceInput> experience = [];

    public string? DisplayName { get; set; }

    public string? Title { get; set; }

    public string? City { get; set; }

    public string? Summary { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Website { get; set; }

    public bool ConsentToShowContact { get; set; }

    /// <summary>Service rows, each a catalog service chosen from a list (prototype editorParts).</summary>
    /// <remarks>Form binding sets a list to null when the post has no entries for it, so null becomes empty.</remarks>
    public List<ServiceInput> Services { get => services; set => services = value ?? []; }

    /// <remarks>A new profile posts no experience rows; binding then sets null, which becomes empty.</remarks>
    public List<ExperienceInput> Experience { get => experience; set => experience = value ?? []; }

    /// <summary>The version the form was loaded from; a save based on an older one is refused (PE05).</summary>
    public int Version { get; set; }

    public static ProfileForm From(AccountantProfile profile)
    {
        var form = new ProfileForm
        {
            DisplayName = profile.DisplayName,
            Title = profile.Title,
            City = profile.City,
            Summary = profile.Summary,
            Email = profile.Email,
            Phone = profile.Phone,
            Website = profile.Website,
            ConsentToShowContact = profile.ConsentToShowContact,
            Services = profile.Services
                .Select(s => new ServiceInput { Name = s.Name, Description = s.Description })
                .ToList(),
            Experience = profile.Experience
                .Select(e => new ExperienceInput
                {
                    Role = e.Role,
                    Organization = e.Organization,
                    Start = e.Start.ToString(),
                    End = e.End?.ToString(),
                    Current = e.IsCurrent,
                })
                .ToList(),
            Version = profile.Version,
        };
        return form;
    }

    /// <summary>
    /// A new accountant's starting point, as in the prototype: their name from the sign-in provider, and no services or
    /// experience rows until they add some. The contact email starts empty, since it may differ from the sign-in email.
    /// </summary>
    public static ProfileForm StartingFrom(string? displayName) => new() { DisplayName = displayName };

    /// <summary>The prototype's listing checklist (listingSteps), worked out from the values as typed.</summary>
    public IReadOnlyList<ListingStep> ListingSteps() =>
    [
        new("display name", "Display name", !string.IsNullOrWhiteSpace(DisplayName), "f-name"),
        new("professional title", "Professional title", !string.IsNullOrWhiteSpace(Title), "f-title"),
        new("career summary", "Career summary", !string.IsNullOrWhiteSpace(Summary), "f-summary"),
        new("at least one service", "At least one service", Services.Any(s => ServiceCatalog.Find(s.Name) is not null),
            Services.Count > 0 ? "f-svc-0-name" : "add-svc"),
        new("email", "Email", !string.IsNullOrWhiteSpace(Email), "f-email"),
        new("agreement to show contact details", "Agreement to show your contact details", ConsentToShowContact, "f-consent"),
    ];
}

/// <summary>One step towards being listed (BR01): its name in messages, its checklist label and the field to fix.</summary>
public sealed record ListingStep(string Name, string Label, bool Done, string TargetId);

public sealed class ServiceInput
{
    /// <summary>A catalog service name; empty until one is chosen.</summary>
    public string? Name { get; set; }

    public string? Description { get; set; }
}

public sealed class ExperienceInput
{
    public string? Role { get; set; }

    public string? Organization { get; set; }

    /// <summary><c>YYYY-MM</c>, as a month input sends it.</summary>
    public string? Start { get; set; }

    public string? End { get; set; }

    public bool Current { get; set; }

    public bool IsBlank =>
        string.IsNullOrWhiteSpace(Role) && string.IsNullOrWhiteSpace(Organization)
        && string.IsNullOrWhiteSpace(Start) && string.IsNullOrWhiteSpace(End) && !Current;
}
