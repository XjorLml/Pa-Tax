using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using TaxBuddy.Web.Data;
using TaxBuddy.Web.Domain;
using TaxBuddy.Web.Services;

namespace TaxBuddy.Web.Account;

/// <summary>
/// Sign-in wiring (F04; ADR 006; SDD sign-in, sessions and authorization): ASP.NET Core Identity with external
/// sign-ins only, Identity's cookies, and Google when it is configured.
/// </summary>
public static class AccountSetup
{
    public const string GoogleScheme = GoogleDefaults.AuthenticationScheme;

    /// <summary>Proposed in F04: a persistent session renewed on use (Identity's default). The owner can change it.</summary>
    public static readonly TimeSpan SessionLifetime = TimeSpan.FromDays(14);

    /// <summary>
    /// Google is registered only with both values present, because a registered handler without them fails on every
    /// request. Without them the sign-in page says Google sign-in isn't set up (GS08).
    /// </summary>
    public static bool IsGoogleConfigured(IConfiguration configuration) =>
        !string.IsNullOrWhiteSpace(configuration["Authentication:Google:ClientId"])
        && !string.IsNullOrWhiteSpace(configuration["Authentication:Google:ClientSecret"]);

    public static void AddTaxBuddyAccounts(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var development = builder.Environment.IsDevelopment();

        services.AddCascadingAuthenticationState();
        services.AddScoped<AccountService>();

        var authentication = services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        });
        authentication.AddIdentityCookies();

        services.AddIdentityCore<TaxBuddyUser>(options =>
            {
                // External sign-ins only (ADR 006): there are no passwords, and the provider has confirmed the email.
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.User.RequireUniqueEmail = false;
            })
            .AddEntityFrameworkStores<TaxBuddyDbContext>()
            .AddSignInManager()
            .AddClaimsPrincipalFactory<TaxBuddyClaimsFactory>();

        // SDD session: HTTPS-only outside development, unreadable by scripts.
        var secure = development ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = secure;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.ExpireTimeSpan = SessionLifetime;
            options.SlidingExpiration = true;
            options.LoginPath = "/signin";
            options.LogoutPath = AccountEndpoints.SignOutPath;
            options.ReturnUrlParameter = "returnUrl";
        });
        services.ConfigureExternalCookie(options =>
        {
            // Holds a first sign-in until registration completes; Identity's default lifetime is a few minutes.
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = secure;
        });

        if (IsGoogleConfigured(builder.Configuration))
        {
            authentication.AddGoogle(GoogleScheme, options =>
            {
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.Events.OnAccessDenied = ReturnToStartingPage;
                options.Events.OnRemoteFailure = ShowSignInFailure;
            });

            // Read when the handler is first used, so the host's secret store and test settings both apply.
            services.AddOptions<GoogleOptions>(GoogleScheme).Configure<IConfiguration>((options, configuration) =>
            {
                options.ClientId = configuration["Authentication:Google:ClientId"]!;
                options.ClientSecret = configuration["Authentication:Google:ClientSecret"]!;
            });
        }
    }

    /// <summary>The visitor cancelled on Google's page: back where they started, not signed in (GS04; SDD navigation).</summary>
    private static Task ReturnToStartingPage(AccessDeniedContext context)
    {
        context.Response.Redirect(LocalAddress.OrHome(ReturnAddress(context.Properties)));
        context.HandleResponse();
        return Task.CompletedTask;
    }

    /// <summary>Google or the state check failed: the sign-in page with an error and a way to try again (GS04).</summary>
    private static Task ShowSignInFailure(RemoteFailureContext context)
    {
        // SDD diagnostics: the error category only, never the email, name or provider message.
        context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("TaxBuddy.Web.Account.SignIn")
            .LogWarning("External sign-in failed: {Category}", context.Failure?.GetType().Name ?? "unknown");

        context.Response.Redirect(AccountEndpoints.SignInPageAddress(ReturnAddress(context.Properties), "failed"));
        context.HandleResponse();
        return Task.CompletedTask;
    }

    private static string? ReturnAddress(AuthenticationProperties? properties) =>
        properties?.Items.TryGetValue(AccountEndpoints.ReturnUrlItem, out var value) == true ? value : null;
}

/// <summary>Claims for the session cookie: the display name and the account's role, for the header and later checks.</summary>
public sealed class TaxBuddyClaimsFactory(UserManager<TaxBuddyUser> users, IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<TaxBuddyUser>(users, options)
{
    public const string DisplayNameClaim = "taxbuddy:name";

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(TaxBuddyUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim(DisplayNameClaim, user.DisplayName));
        identity.AddClaim(new Claim(ClaimTypes.Role, AccountRoles.ValueOf(user.Role)));
        return identity;
    }
}
