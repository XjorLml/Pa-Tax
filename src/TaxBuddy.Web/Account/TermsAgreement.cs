using System.Security.Claims;
using Microsoft.AspNetCore.Components.Endpoints;
using TaxBuddy.Web.Domain;
using TaxBuddy.Web.Services;

namespace TaxBuddy.Web.Account;

/// <summary>
/// Marks a page that a signed-in person may open before agreeing to the current terms (F11 LP05): the legal pages, the
/// agree page itself, sign-in, registration and the error pages.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class TermsNotRequiredAttribute : Attribute;

/// <summary>
/// PRD R16, decided 2026-11-21: a signed-in person whose agreed version is older than <see cref="LegalText.CurrentVersion"/>
/// agrees again before continuing. Page requests only; form posts, endpoints and Blazor's own addresses pass through,
/// because this is a legal prompt, not access control.
/// </summary>
public static class TermsAgreement
{
    public const string AgreePath = "/account/agree";

    public static string AgreePageAddress(string? returnUrl) =>
        AgreePath + "?returnUrl=" + Uri.EscapeDataString(LocalAddress.OrHome(returnUrl));

    /// <summary>True when the session carries the current version. A session from before F11 has no version.</summary>
    public static bool HasAgreedToCurrent(ClaimsPrincipal user) =>
        user.FindFirstValue(TaxBuddyClaimsFactory.TermsVersionClaim) == LegalText.CurrentVersion;

    public static IApplicationBuilder UseTermsAgreement(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            if (NeedsAgreement(context))
            {
                var current = context.Request.PathBase + context.Request.Path + context.Request.QueryString;
                context.Response.Redirect(AgreePageAddress(current));
                return;
            }

            await next(context);
        });

    private static bool NeedsAgreement(HttpContext context)
    {
        if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method))
        {
            return false;
        }

        if (context.User.Identity?.IsAuthenticated != true || HasAgreedToCurrent(context.User))
        {
            return false;
        }

        // Routing runs before this middleware in a WebApplication, so the page and its attributes are known.
        var metadata = context.GetEndpoint()?.Metadata;
        return metadata?.GetMetadata<ComponentTypeMetadata>() is not null
               && metadata.GetMetadata<TermsNotRequiredAttribute>() is null;
    }
}
