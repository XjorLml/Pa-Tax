using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaxBuddy.Web.Data;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Account;

/// <summary>
/// The signed-in client's name, sign-in email and details, as Account shows them (F17), and the accountants who can see
/// the details: those the client has a service with, who see them on their page for the client and in requests.
/// </summary>
public sealed record ClientDetailsView(string Name, string? Email, ClientDetails Details, IReadOnlyList<string> SharedWith);

public enum ClientDetailsOutcome
{
    Saved,

    /// <summary>A field can't be saved; nothing was stored.</summary>
    Invalid,

    /// <summary>Not signed in as a client (or the account is gone).</summary>
    NotAClient,

    /// <summary>The database couldn't save it; nothing was stored.</summary>
    Failed,
}

public sealed record ClientDetailsResult(ClientDetailsOutcome Outcome, IReadOnlyDictionary<string, string> Problems)
{
    public static readonly IReadOnlyDictionary<string, string> NoProblems = new Dictionary<string, string>();
}

/// <summary>
/// F17 (PRD R25): a client keeps their name and optional details on Account. Saving renews the session, so a changed name
/// is in the header at once. Pages use this rather than Identity's managers (SDD dependency rules).
/// </summary>
public sealed class ClientDetailsService(
    UserManager<TaxBuddyUser> users,
    SignInManager<TaxBuddyUser> signIn,
    IDbContextFactory<TaxBuddyDbContext> contexts,
    ILogger<ClientDetailsService> logger)
{
    private static readonly EventId Saved = new(1701, "ClientDetailsSaved");
    private static readonly EventId SaveFailed = new(1702, "ClientDetailsSaveFailed");

    /// <summary>The client's details, or null for anyone who isn't a signed-in client.</summary>
    public async Task<ClientDetailsView?> LoadAsync(ClaimsPrincipal principal)
    {
        if (await ClientAsync(principal) is not { } user)
        {
            return null;
        }

        await using var db = await contexts.CreateDbContextAsync();
        var accountants = await db.ServiceEngagements.Where(e => e.ClientAccountId == user.Id).Select(e => e.AccountantAccountId).Distinct().ToListAsync();
        var profiles = await db.AccountantProfiles.Where(p => p.OwnerAccountId != null && accountants.Contains(p.OwnerAccountId.Value))
            .ToDictionaryAsync(p => p.OwnerAccountId!.Value, p => p.DisplayName);
        var names = await db.Users.Where(u => accountants.Contains(u.Id) && u.DeletedAt == null)
            .Select(u => new { u.Id, u.DisplayName }).ToListAsync();
        var sharedWith = names.Select(u => profiles.GetValueOrDefault(u.Id) ?? u.DisplayName).Order(StringComparer.CurrentCulture).ToList();
        return new ClientDetailsView(user.DisplayName, user.Email, user.ClientDetails, sharedWith);
    }

    public async Task<ClientDetailsResult> SaveAsync(ClaimsPrincipal principal, ClientDetailsInput input)
    {
        var problems = ClientDetailsRules.Validate(input);
        if (problems.Count > 0)
        {
            return new ClientDetailsResult(ClientDetailsOutcome.Invalid, problems);
        }

        try
        {
            if (await ClientAsync(principal) is not { } user)
            {
                return new ClientDetailsResult(ClientDetailsOutcome.NotAClient, ClientDetailsResult.NoProblems);
            }

            var (name, details) = ClientDetailsRules.Clean(input);
            user.DisplayName = name;
            user.PhoneNumber = details.Phone;
            user.City = details.City;
            user.BusinessName = details.BusinessName;
            user.BusinessType = details.BusinessType;
            user.Industry = details.Industry;
            var updated = await users.UpdateAsync(user);
            if (!updated.Succeeded)
            {
                // SDD diagnostics: Identity's error codes only, never the details.
                logger.LogWarning(SaveFailed, "Saving client details failed: {Codes}", string.Join(",", updated.Errors.Select(e => e.Code)));
                return new ClientDetailsResult(ClientDetailsOutcome.Failed, ClientDetailsResult.NoProblems);
            }

            await signIn.RefreshSignInAsync(user);
            logger.LogInformation(Saved, "Client details saved");
            return new ClientDetailsResult(ClientDetailsOutcome.Saved, ClientDetailsResult.NoProblems);
        }
        catch (Exception ex) when (ex is System.Data.Common.DbException or DbUpdateException or TimeoutException
                                       or InvalidOperationException { InnerException: System.Data.Common.DbException or TimeoutException })
        {
            logger.LogWarning(SaveFailed, "Saving client details failed ({ErrorType})", ex.GetType().Name);
            return new ClientDetailsResult(ClientDetailsOutcome.Failed, ClientDetailsResult.NoProblems);
        }
    }

    private async Task<TaxBuddyUser?> ClientAsync(ClaimsPrincipal principal) =>
        await users.GetUserAsync(principal) is { Role: AccountRole.Client, DeletedAt: null } user ? user : null;
}
