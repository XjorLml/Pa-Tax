using System.Globalization;
using Microsoft.AspNetCore.WebUtilities;
using TaxBuddy.Web.Application.Directory;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Components.Shared;

/// <summary>Presentation helpers: page addresses and display text. No business rules live here.</summary>
public static class Display
{
    public static string ProfilePath(AccountantId id) => $"/accountants/{id}";

    /// <summary>
    /// The directory address for a query (SDD navigation: <c>q</c>, <c>service</c>, <c>sort</c>, <c>page</c>). Defaults are left out,
    /// so each list has one address that Back, reload and shared links reproduce.
    /// </summary>
    public static string DirectoryPath(DirectoryQuery query, int? page = null)
    {
        var values = new Dictionary<string, string?>();
        if (query.SearchText is { } text)
        {
            values["q"] = text;
        }

        if (query.Service is { } service)
        {
            values["service"] = service;
        }

        if (query.Sort == DirectorySort.Rating)
        {
            values["sort"] = DirectoryQuery.RatingSortValue;
        }

        var effectivePage = page ?? query.Page;
        if (effectivePage > 1)
        {
            values["page"] = effectivePage.ToString(CultureInfo.InvariantCulture);
        }

        return QueryHelpers.AddQueryString("/", values);
    }

    public static string Initials(string name)
    {
        var letters = name
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => char.IsUpper(w[0]))
            .Take(2)
            .Select(w => w[0]);
        var initials = string.Concat(letters);
        return initials.Length > 0 ? initials : "?";
    }

    /// <summary>The prototype's avatar tone (<c>tone()</c> in prototypes/index.html): t1, t2 or t3 from the name.</summary>
    public static string Tone(string name)
    {
        var h = 0;
        foreach (var c in name)
        {
            h = (h * 31 + c) % 997;
        }

        return "t" + (h % 3 + 1);
    }

    public static string YearsOfExperience(int years) => years == 1 ? "1 year's experience" : $"{years} years' experience";

    /// <summary>The card fact (prototype cardHTML): "1 highlight", "3 highlights".</summary>
    public static string HighlightCount(int count) => count == 1 ? "1 highlight" : $"{count} highlights";

    /// <summary>The average to one decimal, "4.5" (BR10).</summary>
    public static string Rating(decimal average) => average.ToString("0.0", CultureInfo.InvariantCulture);

    /// <summary>"1 review", "12 reviews" (prototype countText).</summary>
    public static string ReviewCount(int count) => count == 1 ? "1 review" : $"{count} reviews";

    /// <summary>The review page for an accountant.</summary>
    public static string ReviewPath(AccountantId id) => $"/accountants/{id}/review";

    public static string AccountantCount(int count) => count == 1 ? "1 accountant" : $"{count} accountants";

    /// <summary>A <c>tel:</c> address with only digits and a leading plus, or null when nothing dialable remains.</summary>
    public static string? TelHref(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return null;
        }

        var trimmed = phone.Trim();
        var digits = string.Concat(trimmed.Where(char.IsAsciiDigit));
        return digits.Length >= 5 ? "tel:" + (trimmed.StartsWith('+') ? "+" : "") + digits : null;
    }

    public static string FirstName(string name) => name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? name;
}
