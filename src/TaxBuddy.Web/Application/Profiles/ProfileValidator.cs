using System.Globalization;
using System.Net.Mail;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Application.Profiles;

/// <summary>The profile's values after validation, ready for the domain.</summary>
public sealed record ValidatedProfile(
    string DisplayName,
    string Title,
    string? City,
    string Summary,
    string Email,
    string? Phone,
    string? Website,
    bool ConsentToShowContact,
    IReadOnlyList<(string Name, string Description)> Services,
    IReadOnlyList<(string Role, string Organization, YearMonth Start, YearMonth? End)> Experience)
{
    /// <summary>Portfolio highlights (F08), in the order typed.</summary>
    public IReadOnlyList<(string Title, string? ClientType, string Work, string? Result)> Highlights { get; init; } = [];
}

/// <summary>
/// Checks the editor's input before anything is saved (F05 PE03; PRD D04 limits). Messages follow the prototype's
/// validateFormat (F02 BD10). Every problem is keyed by its field's form path, so the page shows it beside the field.
/// Fields needed to be listed may be empty: an unfinished profile is saved, unlisted (BR01).
/// </summary>
public static class ProfileValidator
{
    public static (ValidatedProfile? Profile, IReadOnlyDictionary<string, string> Errors) Validate(ProfileForm form)
    {
        var errors = new Dictionary<string, string>(StringComparer.Ordinal);

        var displayName = Text(form.DisplayName, ProfileLimits.DisplayName, nameof(ProfileForm.DisplayName), "Display name", errors);
        var title = Text(form.Title, ProfileLimits.Title, nameof(ProfileForm.Title), "Professional title", errors);
        var city = Optional(form.City, ProfileLimits.City, nameof(ProfileForm.City), "City", errors);
        var summary = (form.Summary ?? "").Trim();
        if (summary.Length > ProfileLimits.Summary)
        {
            errors[nameof(ProfileForm.Summary)] =
                $"Keep the career summary under {ProfileLimits.Summary.ToString("N0", CultureInfo.InvariantCulture)} characters.";
        }

        var email = Text(form.Email, ProfileLimits.Email, nameof(ProfileForm.Email), "Email", errors);
        if (email.Length > 0 && !IsEmail(email))
        {
            errors.TryAdd(nameof(ProfileForm.Email), "Enter an email address like name@example.com.");
        }

        var phone = Optional(form.Phone, ProfileLimits.Phone, nameof(ProfileForm.Phone), "Phone", errors);
        if (phone is not null && !phone.All(c => char.IsAsciiDigit(c) || c is ' ' or '+' or '-' or '(' or ')' or '.'))
        {
            errors.TryAdd(nameof(ProfileForm.Phone), "Use only digits, spaces and + - ( ) .");
        }

        var website = Website(form.Website, errors);

        var experience = new List<(string, string, YearMonth, YearMonth?)>();
        var rows = form.Experience.Select((row, index) => (row, index)).Where(r => !r.row.IsBlank).ToList();
        if (rows.Count > ProfileLimits.ExperienceEntries)
        {
            errors.TryAdd(nameof(ProfileForm.Experience), $"Keep to {ProfileLimits.ExperienceEntries} experience entries or fewer.");
        }

        foreach (var (row, i) in rows)
        {
            var n = i + 1;
            var role = Text(row.Role, ProfileLimits.ExperienceText, $"Experience[{i}].Role", $"Experience {n}: the role", errors);
            if (role.Length == 0)
            {
                errors.TryAdd($"Experience[{i}].Role", $"Experience {n}: add a role.");
            }

            var organization = Text(row.Organization, ProfileLimits.ExperienceText, $"Experience[{i}].Organization", $"Experience {n}: the company", errors);
            if (organization.Length == 0)
            {
                errors.TryAdd($"Experience[{i}].Organization", $"Experience {n}: add a company, or \"Self-employed\".");
            }

            var start = Month(row.Start, $"Experience[{i}].Start", errors, $"Experience {n}: add a start month.", $"Experience {n}: write the start month as YYYY-MM, for example 2021-03.");
            YearMonth? end = null;
            if (!row.Current)
            {
                end = Month(row.End, $"Experience[{i}].End", errors, $"Experience {n}: add an end month, or tick \"I work here now\".",
                    $"Experience {n}: write the end month as YYYY-MM, for example 2021-03.");
                if (start is { } s && end is { } e && e.CompareTo(s) < 0)
                {
                    errors.TryAdd($"Experience[{i}].End", $"Experience {n}: the end month must be after the start month.");
                }
            }

            if (start is { } startMonth && role.Length > 0 && organization.Length > 0)
            {
                experience.Add((role, organization, startMonth, end));
            }
        }

        var highlights = Highlights(form, errors);

        var services = new List<(string, string)>();
        for (var i = 0; i < form.Services.Count; i++)
        {
            var row = form.Services[i];
            var n = i + 1;
            var nameKey = $"Services[{i}].Name";
            if (string.IsNullOrWhiteSpace(row.Name))
            {
                errors.TryAdd(nameKey, $"Service {n}: choose a service, or remove the row.");
                continue;
            }

            if (ServiceCatalog.Find(row.Name) is not { } name)
            {
                errors.TryAdd(nameKey, $"Service {n}: choose a service from the list.");
                continue;
            }

            if (services.Any(s => s.Item1 == name))
            {
                errors.TryAdd(nameKey, $"Service {n}: {name} is already listed. Choose another, or remove the row.");
                continue;
            }

            var description = Optional(row.Description, ProfileLimits.ServiceDescription, $"Services[{i}].Description",
                $"Service {n}: the description", errors) ?? "";
            services.Add((name, description));
        }

        if (errors.Count > 0)
        {
            return (null, errors);
        }

        return (new ValidatedProfile(displayName, title, city, summary, email, phone, website, form.ConsentToShowContact,
            services, experience) { Highlights = highlights }, errors);
    }

    /// <summary>
    /// F08 (BR13): rows left completely blank are dropped; any other row needs a title and what was done, as in the
    /// prototype's validateFormat.
    /// </summary>
    private static List<(string, string?, string, string?)> Highlights(ProfileForm form, Dictionary<string, string> errors)
    {
        var highlights = new List<(string, string?, string, string?)>();
        var rows = form.Highlights.Select((row, index) => (row, index)).Where(r => !r.row.IsBlank).ToList();
        if (rows.Count > ProfileLimits.Highlights)
        {
            errors.TryAdd(nameof(ProfileForm.Highlights), $"Keep to {ProfileLimits.Highlights} highlights or fewer.");
        }

        foreach (var (row, i) in rows)
        {
            var n = i + 1;
            var title = Text(row.Title, ProfileLimits.HighlightTitle, $"Highlights[{i}].Title", $"Highlight {n}: the title", errors);
            if (title.Length == 0)
            {
                errors.TryAdd($"Highlights[{i}].Title", $"Highlight {n}: add a title.");
            }

            var clientType = Optional(row.ClientType, ProfileLimits.HighlightClientType, $"Highlights[{i}].ClientType",
                $"Highlight {n}: the type of client", errors);
            var work = Text(row.Work, ProfileLimits.HighlightWork, $"Highlights[{i}].Work", $"Highlight {n}: what you did", errors);
            if (work.Length == 0)
            {
                errors.TryAdd($"Highlights[{i}].Work", $"Highlight {n}: describe what you did.");
            }

            var result = Optional(row.Result, ProfileLimits.HighlightResult, $"Highlights[{i}].Result", $"Highlight {n}: the result", errors);
            if (title.Length > 0 && work.Length > 0)
            {
                highlights.Add((title, clientType, work, result));
            }
        }

        return highlights;
    }

    /// <summary>Text that may be empty; too long is an error.</summary>
    private static string Text(string? value, int max, string key, string label, Dictionary<string, string> errors)
    {
        var trimmed = (value ?? "").Trim();
        if (trimmed.Length > max)
        {
            errors.TryAdd(key, $"{label} can be at most {max} characters (now {trimmed.Length}).");
        }

        return trimmed;
    }

    private static string? Optional(string? value, int max, string key, string label, Dictionary<string, string> errors)
    {
        var trimmed = Text(value, max, key, label, errors);
        return trimmed.Length == 0 ? null : trimmed;
    }

    private static bool IsEmail(string value) =>
        !value.Any(char.IsWhiteSpace)
        && MailAddress.TryCreate(value, out var address)
        && address.Address == value
        && address.Host.Contains('.', StringComparison.Ordinal);

    /// <summary>Optional; "example.com" becomes "https://example.com". Only http and https addresses are accepted.</summary>
    private static string? Website(string? value, Dictionary<string, string> errors)
    {
        var trimmed = Optional(value, ProfileLimits.Website, nameof(ProfileForm.Website), "Website", errors);
        if (trimmed is null)
        {
            return null;
        }

        var withScheme = trimmed.Contains("://", StringComparison.Ordinal) ? trimmed : "https://" + trimmed;
        if (!Uri.TryCreate(withScheme, UriKind.Absolute, out var uri)
            || uri.Scheme is not ("http" or "https")
            || !uri.Host.Contains('.', StringComparison.Ordinal)
            || withScheme.Any(char.IsWhiteSpace))
        {
            errors.TryAdd(nameof(ProfileForm.Website), "Enter a web address like example.com.");
            return null;
        }

        if (withScheme.Length > ProfileLimits.Website)
        {
            errors.TryAdd(nameof(ProfileForm.Website), $"Website can be at most {ProfileLimits.Website} characters.");
            return null;
        }

        return withScheme;
    }

    private static YearMonth? Month(string? value, string key, Dictionary<string, string> errors, string missing, string malformed)
    {
        var trimmed = (value ?? "").Trim();
        if (trimmed.Length == 0)
        {
            errors.TryAdd(key, missing);
            return null;
        }

        if (DateOnly.TryParseExact(trimmed + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            && date.Year >= 1900)
        {
            return YearMonth.From(date);
        }

        errors.TryAdd(key, malformed);
        return null;
    }
}
