using Microsoft.AspNetCore.Diagnostics;

namespace TaxBuddy.Web.Services;

/// <summary>
/// Return addresses carried through forms and sign-in (SDD: return address after sign-in is local only, so a crafted
/// link can't send a visitor to another site). Used by the theme switch (F02 BD09) and the account endpoints (F04).
/// </summary>
public static class LocalAddress
{
    /// <summary>
    /// True for an address on this site: it starts with one slash and contains no backslash or control character.
    /// </summary>
    public static bool IsLocal(string? address) =>
        !string.IsNullOrEmpty(address)
        && address[0] == '/'
        && (address.Length == 1 || address[1] is not ('/' or '\\'))
        && !address.Contains('\\')
        && !address.Any(char.IsControl);

    /// <summary>The address when it is local; otherwise the directory.</summary>
    public static string OrHome(string? address) => IsLocal(address) ? address! : "/";

    /// <summary>
    /// The current path and query, or the original address when a not-found or error page is shown in its place.
    /// </summary>
    public static string CurrentFor(HttpContext? context)
    {
        if (context is null)
        {
            return "/";
        }

        var reExecuted = context.Features.Get<IStatusCodeReExecuteFeature>();
        var address = reExecuted is not null
            ? reExecuted.OriginalPathBase + reExecuted.OriginalPath + reExecuted.OriginalQueryString
            : context.Request.PathBase + context.Request.Path + context.Request.QueryString;
        return OrHome(address);
    }
}
