using Microsoft.AspNetCore.DataProtection;

namespace TaxBuddy.Web.Services;

/// <summary>
/// What the deployed site needs from the app (ADR 012: a Docker container on an OCI Compute VM, behind Caddy; RELEASE).
/// Forwarded headers need no code here: the container sets <c>ASPNETCORE_FORWARDEDHEADERS_ENABLED=true</c>, which makes
/// ASP.NET Core trust Caddy's X-Forwarded-Proto and X-Forwarded-For, so sign-in builds https callback addresses.
/// </summary>
public static class DeploymentSetup
{
    public const string KeysPathSetting = "DataProtection:KeysPath";
    public const string VersionSetting = "Site:Version";
    public const string HealthPath = "/health";

    /// <summary>
    /// SDD keys and secrets: the keys that protect session cookies, form tokens and email links are kept in
    /// <see cref="KeysPathSetting"/> (the container's volume) when it is set, so a redeploy doesn't sign everyone out or
    /// break open forms and links. Without it the app keeps them where ASP.NET Core does by default (development).
    /// </summary>
    public static void AddTaxBuddyDeployment(this WebApplicationBuilder builder)
    {
        if (builder.Configuration[KeysPathSetting] is { Length: > 0 } keysPath)
        {
            builder.Services.AddDataProtection()
                .SetApplicationName("TaxBuddy")
                .PersistKeysToFileSystem(new DirectoryInfo(keysPath));
        }
    }

    /// <summary>
    /// RELEASE: the health address for the smoke test and monitoring, naming the deployed build (<see cref="VersionSetting"/>,
    /// the git commit the image was built from). It doesn't touch the database, so a database outage doesn't restart the site.
    /// </summary>
    public static IEndpointRouteBuilder MapHealth(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(HealthPath, (HttpContext context, IConfiguration configuration) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            return Results.Json(new { status = "ok", version = configuration[VersionSetting] is { Length: > 0 } version ? version : "dev" });
        });
        return endpoints;
    }
}
