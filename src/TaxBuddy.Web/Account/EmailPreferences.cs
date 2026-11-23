using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaxBuddy.Web.Data;
using TaxBuddy.Web.Services;
using TaxBuddy.Web.Services.Email;

namespace TaxBuddy.Web.Account;

/// <summary>An account's email setting as the pages show it (F14, R08).</summary>
public sealed record EmailSetting(string? Address, bool On);

/// <summary>
/// Turning new-message emails on and off (F14, PRD R08): from Account when signed in, or from the email's signed link
/// without signing in. Pages use this rather than the database (SDD dependency rules).
/// </summary>
public sealed class EmailPreferences(IDbContextFactory<TaxBuddyDbContext> contexts, EmailLinks links)
{
    public const string EndpointPath = "/account/email";

    public async Task<EmailSetting?> GetAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        await using var db = await contexts.CreateDbContextAsync(cancellationToken);
        return await db.Users.Where(u => u.Id == accountId && u.DeletedAt == null)
            .Select(u => new EmailSetting(u.Email, u.EmailNotifications))
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>False when the account doesn't exist or was deleted.</summary>
    public async Task<bool> SetAsync(Guid accountId, bool on, CancellationToken cancellationToken = default)
    {
        await using var db = await contexts.CreateDbContextAsync(cancellationToken);
        return await db.Users.Where(u => u.Id == accountId && u.DeletedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.EmailNotifications, on), cancellationToken) == 1;
    }

    /// <summary>The account a turn-off link names, when its token is genuine.</summary>
    public Guid? AccountFromToken(string? token) => links.TryRead(token, out var id) ? id : null;

    public string Token(Guid accountId) => links.Token(accountId);

    public static Guid? SignedInAccount(ClaimsPrincipal user) =>
        user.Identity?.IsAuthenticated == true && Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}

/// <summary>The email setting's form address (F14).</summary>
public static class EmailPreferenceEndpoints
{
    /// <summary>
    /// <c>POST /account/email</c>: <c>on</c> ("true" or "false"), an optional link token <c>t</c>, and <c>returnUrl</c>. The
    /// signed-in account wins over a token. Antiforgery applies, as for every form ([FromForm]).
    /// </summary>
    public static IEndpointRouteBuilder MapEmailPreferences(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(EmailPreferences.EndpointPath, async ([FromForm] string? on, [FromForm] string? t, [FromForm] string? returnUrl,
            HttpContext context, EmailPreferences preferences) =>
        {
            if (on is not ("true" or "false"))
            {
                return Results.BadRequest();
            }

            if ((EmailPreferences.SignedInAccount(context.User) ?? preferences.AccountFromToken(t)) is not { } account)
            {
                return Results.BadRequest();
            }

            await preferences.SetAsync(account, on == "true");
            return Results.LocalRedirect(LocalAddress.OrHome(returnUrl));
        });
        return endpoints;
    }
}
