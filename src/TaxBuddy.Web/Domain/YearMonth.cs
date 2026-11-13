using System.Globalization;

namespace TaxBuddy.Web.Domain;

/// <summary>
/// A calendar month without a time zone, used for experience start and end (PRD BR07, D04; SDD: YYYY-MM values).
/// </summary>
public readonly record struct YearMonth : IComparable<YearMonth>
{
    public YearMonth(int year, int month)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(year, 1900);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(year, 9999);
        ArgumentOutOfRangeException.ThrowIfLessThan(month, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(month, 12);
        Year = year;
        Month = month;
    }

    public int Year { get; }

    public int Month { get; }

    /// <summary>Parses the <c>YYYY-MM</c> form.</summary>
    public static YearMonth Parse(string text)
    {
        if (DateOnly.TryParseExact(text + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return new YearMonth(date.Year, date.Month);
        }

        throw new FormatException("Expected a month in the form YYYY-MM.");
    }

    public static YearMonth From(DateOnly date) => new(date.Year, date.Month);

    public DateOnly FirstDay => new(Year, Month, 1);

    /// <summary>Whole months from this month to <paramref name="other"/>; negative when <paramref name="other"/> is earlier.</summary>
    public int MonthsUntil(YearMonth other) => (other.Year - Year) * 12 + (other.Month - Month);

    public int CompareTo(YearMonth other) => MonthsUntil(other) switch { > 0 => -1, < 0 => 1, _ => 0 };

    /// <summary>English display such as "Mar 2021" (PRD: English only).</summary>
    public string ToDisplayString() => FirstDay.ToString("MMM yyyy", CultureInfo.InvariantCulture);

    public override string ToString() => $"{Year:D4}-{Month:D2}";
}
