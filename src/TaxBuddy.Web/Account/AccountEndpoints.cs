using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaxBuddy.Web.Data;
using TaxBuddy.Web.Services;

namespace TaxBuddy.Web.Account;

/// <summary>
/// Account endpoints of the one web app (SDD architecture: Account/), not a public API. Forms post here with
/// antiforgery tokens; every redirect goes to a local address only.
/// </summary>
public static class AccountEndpoints
{
    public const string ExternalSignInPath = "/account/external-signin";
    public const string CallbackPath = "/account/signin/callback";
    public const string SignOutPath = "/account/signout";
    public const string RegisterPath = "/register";

    /// <summary>The starting page, kept in the provider's protected state so cancel can return to it.</summary>
    public const string ReturnUrlItem = "returnUrl";

    public static string SignInPageAddress(string? returnUrl, string? error = null)
    {
        var address = "/signin?returnUrl=" + Uri.EscapeDataString(LocalAddress.OrHome(returnUrl));
        return error is null ? address : address + "&error=" + Uri.EscapeDataString(error);
    }

    public static string RegisterPageAddress(string? returnUrl) =>
        RegisterPath + "?returnUrl=" + Uri.EscapeDataString(LocalAddress.OrHome(returnUrl));

    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // "Continue with Google": [FromForm] requires a valid antiforgery token (GS05).
        endpoints.MapPost(ExternalSignInPath, async (
            [FromForm] string? provider, [FromForm] string? returnUrl, SignInManager<TaxBuddyUser> signIn) =>
        {
            var schemes = await signIn.GetExternalAuthenticationSchemesAsync();
            if (schemes.FirstOrDefault(s => s.Name == provider) is not { } scheme)
            {
                return Results.BadRequest();
            }

            var local = LocalAddress.OrHome(returnUrl);
            var properties = signIn.ConfigureExternalAuthenticationProperties(
                scheme.Name, CallbackPath + "?returnUrl=" + Uri.EscapeDataString(local));
            properties.Items[ReturnUrlItem] = local;
            return Results.Challenge(properties, [scheme.Name]);
        });

        // Back from the provider: a known sign-in continues; a first sign-in goes to registration (GS01, GS03).
        endpoints.MapGet(CallbackPath, async (string? returnUrl, SignInManager<TaxBuddyUser> signIn) =>
        {
            var local = LocalAddress.OrHome(returnUrl);
            if (await signIn.GetExternalLoginInfoAsync() is not { } info)
            {
                return Results.LocalRedirect(SignInPageAddress(local, "failed"));
            }

            var result = await signIn.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                return Results.LocalRedirect(local);
            }

            return result.IsLockedOut || result.IsNotAllowed
                ? Results.LocalRedirect(SignInPageAddress(local, "failed"))
                : Results.LocalRedirect(RegisterPageAddress(local));
        });

        // "Sign out" ends the session in this browser (GS06).
        endpoints.MapPost(SignOutPath, async ([FromForm] string? returnUrl, SignInManager<TaxBuddyUser> signIn) =>
        {
            await signIn.SignOutAsync();
            return Results.LocalRedirect("/");
        });

        return endpoints;
    }
}
