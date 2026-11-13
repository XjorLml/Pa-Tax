namespace TaxBuddy.Web.Domain;

/// <summary>The current instant. Replaced by a fixed clock in tests (SDD: interfaces for the clock).</summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

/// <summary>
/// The one place that turns instants into Asia/Manila dates and times for display (PRD BR07).
/// </summary>
public static class ManilaTime
{
    private static readonly TimeZoneInfo Zone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");

    public static DateTimeOffset ToManila(DateTimeOffset instant) => TimeZoneInfo.ConvertTime(instant, Zone);

    public static DateOnly Today(IClock clock) => DateOnly.FromDateTime(ToManila(clock.UtcNow).DateTime);
}
