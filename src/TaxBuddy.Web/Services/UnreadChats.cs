using System.Security.Claims;
using TaxBuddy.Web.Application.Chat;
using TaxBuddy.Web.Application.Directory;

namespace TaxBuddy.Web.Services;

/// <summary>
/// The unread-message count for the navigation (F07; prototype <c>totalUnread</c>): read once per request and shared by
/// the site bar and the phone tab bar. Zero for visitors, and when it can't be read: a count never breaks a page.
/// </summary>
public sealed class UnreadChats(ChatService chat)
{
    private Task<int>? count;

    public Task<int> CountAsync(ClaimsPrincipal? user) => count ??= ReadAsync(user);

    private async Task<int> ReadAsync(ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true || !Guid.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id))
        {
            return 0;
        }

        try
        {
            return await chat.UnreadTotalAsync(id);
        }
        catch (DataUnavailableException)
        {
            return 0;
        }
    }
}
