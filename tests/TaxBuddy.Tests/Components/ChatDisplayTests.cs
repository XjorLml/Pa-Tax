using TaxBuddy.Web.Components.Chat;

namespace TaxBuddy.Tests.Components;

/// <summary>The prototype's time formats in Manila time (BR07).</summary>
public sealed class ChatDisplayTests
{
    // 2026-11-24 14:00 in Manila (UTC+8).
    private static readonly DateTimeOffset Now = new(2026, 11, 24, 6, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(0, "10:42 AM")] // Today: the time once (it was "10:42 AM · 10:42 AM" in the service history).
    [InlineData(1, "Yesterday · 10:42 AM")]
    [InlineData(8, "Nov 16 · 10:42 AM")]
    public void A_service_history_time_shows_the_time_once(int daysAgo, string expected)
    {
        var at = new DateTimeOffset(2026, 11, 24, 2, 42, 0, TimeSpan.Zero).AddDays(-daysAgo); // 10:42 AM in Manila.

        Assert.Equal(expected, ChatDisplay.ShortWithTime(at, Now));
    }
}
