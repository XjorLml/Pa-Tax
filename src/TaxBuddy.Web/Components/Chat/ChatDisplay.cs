using System.Globalization;
using System.Security.Claims;
using TaxBuddy.Web.Application.Chat;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Components.Chat;

/// <summary>Chat presentation helpers: addresses, the viewer from the sign-in, and the prototype's time formats (BR07: Manila time).</summary>
public static class ChatDisplay
{
    private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-US");

    public static string ThreadPath(Guid conversationId) => $"/chats/{conversationId:D}";

    public static string NewChatPath(AccountantId profileId) => $"/chats/new/{profileId}";

    /// <summary>The signed-in account and its role, or null for a visitor (or a sign-in without them).</summary>
    public static ChatViewer? ViewerOf(ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true
            || !Guid.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id)
            || AccountRoles.Parse(user.FindFirst(ClaimTypes.Role)?.Value) is not { } role)
        {
            return null;
        }

        return new ChatViewer(id, role);
    }

    /// <summary>"3:05 PM" (prototype <c>fmtTime</c>).</summary>
    public static string Time(DateTimeOffset instant) => ManilaTime.ToManila(instant).ToString("h:mm tt", English);

    /// <summary>The list's time: the time today, "Yesterday", otherwise "Nov 16" (prototype <c>fmtShort</c>).</summary>
    public static string Short(DateTimeOffset instant, DateTimeOffset now) => DayOffset(instant, now) switch
    {
        0 => Time(instant),
        1 => "Yesterday",
        _ => ManilaTime.ToManila(instant).ToString("MMM d", English),
    };

    /// <summary>
    /// A service history time: "3:05 PM" today, otherwise "Yesterday · 3:05 PM" or "Nov 16 · 3:05 PM" (prototype
    /// work timeline: the time is added only when the short form has none).
    /// </summary>
    public static string ShortWithTime(DateTimeOffset instant, DateTimeOffset now) =>
        DayOffset(instant, now) == 0 ? Time(instant) : $"{Short(instant, now)} · {Time(instant)}";

    /// <summary>The thread's day separator: "Today", "Yesterday", otherwise "Mon, Nov 16" (prototype <c>dayLabel</c>).</summary>
    public static string Day(DateTimeOffset instant, DateTimeOffset now) => DayOffset(instant, now) switch
    {
        0 => "Today",
        1 => "Yesterday",
        _ => ManilaTime.ToManila(instant).ToString("ddd, MMM d", English),
    };

    public static DateOnly ManilaDate(DateTimeOffset instant) => DateOnly.FromDateTime(ManilaTime.ToManila(instant).DateTime);

    public static string Count(int value) => value.ToString("N0", English);

    private static int DayOffset(DateTimeOffset instant, DateTimeOffset now) =>
        ManilaDate(now).DayNumber - ManilaDate(instant).DayNumber;
}
