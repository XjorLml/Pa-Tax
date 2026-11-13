using Microsoft.AspNetCore.Diagnostics;

namespace TaxBuddy.Web.Components.Shared;

/// <summary>
/// Sets an error status on a statically rendered page that shows its own message (for example "no longer available",
/// FT05, or a retry, FT06). Status-code pages are switched off for that response, so the generic "Page not found" page
/// does not replace the page's own content.
/// </summary>
public static class ResponseStatus
{
    public static void Set(HttpContext? context, int statusCode)
    {
        if (context is null || context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = statusCode;
        if (context.Features.Get<IStatusCodePagesFeature>() is { } statusCodePages)
        {
            statusCodePages.Enabled = false;
        }
    }
}
