namespace TaxBuddy.Web.Domain;

/// <summary>
/// What a client keeps about themselves (F17, PRD R25): optional facts shared with the accountants they contact, never
/// public (R11). The name is the account's display name and is handled with these. Blank fields are stored as null.
/// </summary>
public sealed record ClientDetails(string? Phone, string? City, string? BusinessName, string? BusinessType, string? Industry)
{
    public static readonly ClientDetails Empty = new(null, null, null, null, null);

    public bool IsEmpty => Phone is null && City is null && BusinessName is null && BusinessType is null && Industry is null;

    /// <summary>The business as one line, for example "Santos Bakeshop · Sole proprietor · Food and drink".</summary>
    public string? BusinessLine =>
        string.Join(" · ", new[] { BusinessName, BusinessType, Industry }.Where(v => !string.IsNullOrEmpty(v))) is { Length: > 0 } line ? line : null;
}

/// <summary>The details as typed in the form, before they are checked (F17).</summary>
public sealed class ClientDetailsInput
{
    public string? Name { get; set; }

    public string? Phone { get; set; }

    public string? City { get; set; }

    public string? BusinessName { get; set; }

    public string? BusinessType { get; set; }

    public string? Industry { get; set; }
}

/// <summary>The checks and limits for <see cref="ClientDetails"/> (F17 spec), the same as the prototype's.</summary>
public static class ClientDetailsRules
{
    public const int PhoneMin = 7;
    public const int PhoneMax = 20;
    public const int City = 80;
    public const int BusinessName = 120;
    public const int BusinessType = 40;
    public const int Industry = 80;

    public static readonly IReadOnlyList<string> BusinessTypes =
        ["Sole proprietor", "Partnership", "Corporation", "Freelancer or self-employed", "Not registered yet"];

    /// <summary>The field names the messages are keyed by, which are also the form's input names after "Form.".</summary>
    public static class Fields
    {
        public const string Name = nameof(ClientDetailsInput.Name);
        public const string Phone = nameof(ClientDetailsInput.Phone);
        public const string City = nameof(ClientDetailsInput.City);
        public const string BusinessName = nameof(ClientDetailsInput.BusinessName);
        public const string BusinessType = nameof(ClientDetailsInput.BusinessType);
        public const string Industry = nameof(ClientDetailsInput.Industry);
    }

    /// <summary>A message for each field that can't be saved; empty when everything can.</summary>
    public static IReadOnlyDictionary<string, string> Validate(ClientDetailsInput input)
    {
        var problems = new Dictionary<string, string>();
        var name = Trimmed(input.Name);
        if (name is null)
        {
            problems[Fields.Name] = "Enter your name.";
        }
        else if (name.Length > AccountLimits.DisplayName)
        {
            problems[Fields.Name] = $"Use {AccountLimits.DisplayName} characters or fewer.";
        }

        if (Trimmed(input.Phone) is { } phone && !IsPhone(phone))
        {
            problems[Fields.Phone] = "Enter a phone number using digits, spaces, + or -.";
        }

        Limit(problems, Fields.City, input.City, City);
        Limit(problems, Fields.BusinessName, input.BusinessName, BusinessName);
        Limit(problems, Fields.Industry, input.Industry, Industry);
        if (Trimmed(input.BusinessType) is { } type && !BusinessTypes.Contains(type))
        {
            problems[Fields.BusinessType] = "Choose a type from the list.";
        }

        return problems;
    }

    /// <summary>The name and details to store, trimmed, with blanks as null. Call only after <see cref="Validate"/> passes.</summary>
    public static (string Name, ClientDetails Details) Clean(ClientDetailsInput input) =>
        (Trimmed(input.Name)!, new ClientDetails(Trimmed(input.Phone), Trimmed(input.City), Trimmed(input.BusinessName),
            Trimmed(input.BusinessType), Trimmed(input.Industry)));

    /// <summary>7–20 characters of digits, spaces, "+", "-" and parentheses, with at least 7 digits.</summary>
    public static bool IsPhone(string phone) =>
        phone.Length is >= PhoneMin and <= PhoneMax
        && phone.All(c => char.IsAsciiDigit(c) || c is ' ' or '+' or '-' or '(' or ')')
        && phone.Count(char.IsAsciiDigit) >= PhoneMin;

    private static void Limit(Dictionary<string, string> problems, string field, string? value, int max)
    {
        if (Trimmed(value) is { Length: var length } && length > max)
        {
            problems[field] = $"Use {max} characters or fewer.";
        }
    }

    private static string? Trimmed(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
