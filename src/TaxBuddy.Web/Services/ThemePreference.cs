using Microsoft.AspNetCore.Mvc;

namespace TaxBuddy.Web.Services;

/// <summary>The visitor's colour theme choice (DESIGN theme switch, F02 BD09). System follows the browser setting.</summary>
public enum Theme
{
    System,
    Light,
    Dark,
}

/// <summary>
/// Stores the theme choice in a cookie, so the server writes it on the page's <c>html</c> element and the first response
/// already has the right colours, with no script (SDD: browser storage). The switch is a plain form posted here; a
/// successful post redirects back to the page it came from.
/// </summary>
public static class ThemePreference
{
    public const string CookieName = "taxbuddy-theme";

    public const string EndpointPath = "/preferences/theme";

    /// <summary>A year, renewed whenever the visitor chooses again.</summary>
    public static readonly TimeSpan CookieLifetime = TimeSpan.FromDays(365);

    public static Theme Read(HttpContext? context) =>
        Parse(context?.Request.Cookies[CookieName]) ?? Theme.System;

    /// <summary>The form value or cookie value for a choice; null for anything else.</summary>
    public static Theme? Parse(string? value) => value switch
    {
        "system" => Theme.System,
        "light" => Theme.Light,
        "dark" => Theme.Dark,
        _ => null,
    };

    public static string ValueOf(Theme theme) => theme switch
    {
        Theme.Light => "light",
        Theme.Dark => "dark",
        _ => "system",
    };

    public static IEndpointConventionBuilder MapThemePreference(this IEndpointRouteBuilder endpoints) =>
        // [FromForm] makes the endpoint require a valid antiforgery token (SDD: antiforgery on every form).
        endpoints.MapPost(EndpointPath, ([FromForm] string? theme, [FromForm] string? returnUrl, HttpContext context) =>
        {
            if (Parse(theme) is not { } choice)
            {
                return Results.BadRequest();
            }

            Save(context, choice);
            return Results.LocalRedirect(LocalAddress.OrHome(returnUrl));
        });

    private static void Save(HttpContext context, Theme choice)
    {
        var options = new CookieOptions
        {
            Path = "/",
            HttpOnly = true,
            Secure = context.Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            // A display preference with no personal data; it never needs consent to be stored.
            IsEssential = true,
        };

        if (choice == Theme.System)
        {
            // System is the default, so it is stored as no cookie at all.
            context.Response.Cookies.Delete(CookieName, options);
        }
        else
        {
            options.MaxAge = CookieLifetime;
            context.Response.Cookies.Append(CookieName, ValueOf(choice), options);
        }
    }
}
