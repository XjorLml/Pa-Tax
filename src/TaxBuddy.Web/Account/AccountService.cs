using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaxBuddy.Web.Data;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Account;

/// <summary>A first sign-in waiting for registration, as the registration page shows it.</summary>
public sealed record PendingSignIn(string ProviderName, string? Email, string DisplayName);

public enum RegistrationOutcome
{
    SignedIn,
    NoPendingSignIn,
    Failed,
}

/// <summary>
/// What the sign-in and registration pages need (F04). Pages use this rather than Identity's managers, so components
/// never depend on the data layer (SDD dependency rules).
/// </summary>
public sealed class AccountService(
    UserManager<TaxBuddyUser> users,
    SignInManager<TaxBuddyUser> signIn,
    IAuthenticationSchemeProvider schemes,
    IClock clock,
    ILogger<AccountService> logger)
{
    public async Task<bool> IsGoogleAvailableAsync() =>
        await schemes.GetSchemeAsync(AccountSetup.GoogleScheme) is not null;

    public async Task<PendingSignIn?> GetPendingAsync() =>
        await signIn.GetExternalLoginInfoAsync() is { } info
            ? new PendingSignIn(info.ProviderDisplayName ?? info.LoginProvider, EmailOf(info), DisplayNameOf(info))
            : null;

    /// <summary>
    /// Creates the account with the one chosen role (D03) and the agreement time (R16), links the provider sign-in and
    /// signs in. A repeated submit after success signs in to the account the first one created.
    /// </summary>
    public async Task<RegistrationOutcome> RegisterAsync(AccountRole role)
    {
        if (await signIn.GetExternalLoginInfoAsync() is not { } info)
        {
            return RegistrationOutcome.NoPendingSignIn;
        }

        if (await users.FindByLoginAsync(info.LoginProvider, info.ProviderKey) is { } existing)
        {
            return await SignInAsync(existing, info);
        }

        var now = clock.UtcNow;
        var id = Guid.CreateVersion7();
        var user = new TaxBuddyUser
        {
            Id = id,
            UserName = id.ToString("N"),
            Email = EmailOf(info),
            DisplayName = DisplayNameOf(info),
            Role = role,
            AgreedToTermsAt = now,
            AgreedTermsVersion = LegalText.CurrentVersion,
            CreatedAt = now,
        };

        var created = await users.CreateAsync(user);
        if (!created.Succeeded)
        {
            // SDD diagnostics: Identity's error codes only, never the email or name.
            logger.LogWarning("Account creation failed: {Codes}", string.Join(",", created.Errors.Select(e => e.Code)));
            return RegistrationOutcome.Failed;
        }

        try
        {
            var linked = await users.AddLoginAsync(user, new UserLoginInfo(info.LoginProvider, info.ProviderKey, info.ProviderDisplayName));
            if (!linked.Succeeded)
            {
                logger.LogWarning("Linking the sign-in failed: {Codes}", string.Join(",", linked.Errors.Select(e => e.Code)));
                await users.DeleteAsync(user);
                return RegistrationOutcome.Failed;
            }
        }
        catch (DbUpdateException)
        {
            // Two submits at once: the other request linked this sign-in first. Keep its account.
            await users.DeleteAsync(user);
            return await users.FindByLoginAsync(info.LoginProvider, info.ProviderKey) is { } winner
                ? await SignInAsync(winner, info)
                : RegistrationOutcome.Failed;
        }

        return await SignInAsync(user, info);
    }

    private async Task<RegistrationOutcome> SignInAsync(TaxBuddyUser user, ExternalLoginInfo info)
    {
        await signIn.SignInAsync(user, isPersistent: true, info.LoginProvider);
        await signIn.Context.SignOutAsync(IdentityConstants.ExternalScheme);
        return RegistrationOutcome.SignedIn;
    }

    private static string? EmailOf(ExternalLoginInfo info) =>
        Limit(info.Principal.FindFirstValue(ClaimTypes.Email), AccountLimits.Email);

    private static string DisplayNameOf(ExternalLoginInfo info) =>
        Limit(info.Principal.FindFirstValue(ClaimTypes.Name), AccountLimits.DisplayName)
        ?? Limit(info.Principal.FindFirstValue(ClaimTypes.GivenName), AccountLimits.DisplayName)
        ?? "Tax Buddy user";

    private static string? Limit(string? value, int length)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed.Length <= length ? trimmed : trimmed[..length];
    }
}
