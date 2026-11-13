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
    /// The directory address for a query (SDD navigation: <c>q</c>, <c>service</c>, <c>page</c>). Defaults are left out,
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

    public static string YearsOfExperience(int years) => years == 1 ? "1 year's experience" : $"{years} years' experience";

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
