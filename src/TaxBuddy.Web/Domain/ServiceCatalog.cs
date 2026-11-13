namespace TaxBuddy.Web.Domain;

/// <summary>
/// The fixed list of services an accountant can offer (PRD D04, adopted provisionally 2026-11-12).
/// Values are stored and matched exactly; display labels are the same English text.
/// </summary>
public static class ServiceCatalog
{
    public static IReadOnlyList<string> All { get; } =
    [
        "Bookkeeping",
        "Tax filing",
        "Payroll",
        "Financial statements",
        "Audit",
        "Business registration",
        "Tax advisory",
    ];

    /// <summary>Returns the catalog spelling of <paramref name="name"/>, or null when it is not a catalog service.</summary>
    public static string? Find(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var trimmed = name.Trim();
        return All.FirstOrDefault(s => string.Equals(s, trimmed, StringComparison.OrdinalIgnoreCase));
    }
}
