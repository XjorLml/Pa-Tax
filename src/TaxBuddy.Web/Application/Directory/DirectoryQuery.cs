using System.Globalization;
using System.Text.RegularExpressions;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Application.Directory;

/// <summary>
/// What the directory was asked for, already cleaned (SDD navigation, address <c>/</c>):
/// unknown or malformed values are ignored, and search text is capped.
/// </summary>
public sealed partial record DirectoryQuery(string? SearchText, string? Service, int Page)
{
    public const int MaxSearchLength = 100;
    public const int MaxPage = 10_000;

    public static DirectoryQuery Default { get; } = new(null, null, 1);

    public bool IsFiltered => SearchText is not null || Service is not null;

    /// <summary>
    /// Builds a query from raw address values. When several service values arrive, the last one wins: on wide screens a
    /// service button is submitted after the form's own service menu (SDD render modes). "All services" sends an empty
    /// value, which clears the filter.
    /// </summary>
    public static DirectoryQuery From(string? searchText, IReadOnlyList<string?>? services, string? page)
    {
        var lastService = services is { Count: > 0 } ? services[^1] : null;
        return new DirectoryQuery(NormalizeSearch(searchText), ServiceCatalog.Find(lastService), ParsePage(page));
    }

    private static string? NormalizeSearch(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var collapsed = Whitespace().Replace(text.Trim(), " ");
        return collapsed.Length <= MaxSearchLength ? collapsed : collapsed[..MaxSearchLength].TrimEnd();
    }

    private static int ParsePage(string? page) =>
        int.TryParse(page, NumberStyles.None, CultureInfo.InvariantCulture, out var n) && n is >= 1 and <= MaxPage ? n : 1;

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
