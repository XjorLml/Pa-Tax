using System.Net;
using Microsoft.EntityFrameworkCore;
using TaxBuddy.TestSupport;
using TaxBuddy.Web.Account;
using TaxBuddy.Web.Domain;
using TaxBuddy.Web.Tests.Support;
using static TaxBuddy.Web.Tests.Support.FormPosts;

namespace TaxBuddy.Web.Tests;

/// <summary>
/// F15, archived on 2026-11-23 ("Google sign-in is enough"): Microsoft sign-in stays built but hidden. These tests switch
/// it on, so the archived code keeps working through the same registration, sessions and wording; the first page test
/// checks that it stays hidden with its keys stored but the switch off. The test sign-in stands in for Microsoft's pages.
/// </summary>
public sealed class MicrosoftSignInTests(EmptyAppFixture app) : IClassFixture<EmptyAppFixture>
{
    [DatabaseFact]
    public async Task First_Microsoft_sign_in_registers_and_the_account_says_Microsoft()
    {
        await using var factory = new TaxBuddyAppFactory(app.Schema.ConnectionString, withMicrosoft: true);
        var client = factory.CreateHttpsClient();
        var email = $"{Guid.NewGuid():N}@example.test";

        Assert.Equal(HttpStatusCode.NoContent,
            (await client.GetAsync(TestExternalSignIn.Address("ms-key-1", email, "Carla Diaz", AccountSetup.MicrosoftScheme))).StatusCode);
        await client.GetAsync(AccountEndpoints.CallbackPath + "?returnUrl=%2F");
        var register = await client.GetHtmlAsync(AccountEndpoints.RegisterPageAddress("/"));
        Assert.Contains("Signed in with Microsoft. Choose one: an account is either a client or an accountant.", register);

        var fields = HiddenFields(register, "How will you use Tax Buddy?");
        fields["Form.Role"] = AccountRoles.ValueOf(AccountRole.Client);
        fields["Form.Agree"] = "true";
        Assert.Equal("/", Target(await client.PostAsync(AccountEndpoints.RegisterPath, new FormUrlEncodedContent(fields))));

        Assert.Contains("Client · Signed in with Microsoft", await client.GetHtmlAsync("/account"));
        Assert.Contains("Your Tax Buddy sign-in. Your Microsoft account itself is not affected.", await client.GetHtmlAsync("/account/delete"));
        await using var db = app.Schema.CreateContext();
        var user = await db.Users.SingleAsync(u => u.Email == email);
        var login = await db.UserLogins.SingleAsync(l => l.UserId == user.Id);
        Assert.Equal(("Microsoft", "ms-key-1"), (login.LoginProvider, login.ProviderKey));
    }

    [DatabaseFact]
    public async Task A_returning_Microsoft_sign_in_goes_straight_back()
    {
        await using var factory = new TaxBuddyAppFactory(app.Schema.ConnectionString, withMicrosoft: true);
        var email = $"{Guid.NewGuid():N}@example.test";
        await TestAccounts.RegisterAsync(factory.CreateHttpsClient(), AccountRole.Accountant, "Ben Uy", email, AccountSetup.MicrosoftScheme);
        await using var db = app.Schema.CreateContext();
        var key = (await db.UserLogins.SingleAsync(l => l.UserId == db.Users.Single(u => u.Email == email).Id)).ProviderKey;

        var again = factory.CreateHttpsClient();
        await again.GetAsync(TestExternalSignIn.Address(key, email, "Ben Uy", AccountSetup.MicrosoftScheme));
        var callback = await again.GetAsync(AccountEndpoints.CallbackPath + "?returnUrl=%2Faccount");

        Assert.Equal("/account", Target(callback));
        Assert.Contains("Accountant · Signed in with Microsoft", await again.GetHtmlAsync("/account"));
    }
}

/// <summary>F15 without a database: hidden while archived, and when switched on the button, the challenge and the wording.</summary>
public sealed class MicrosoftSignInPageTests : IDisposable
{
    private readonly TaxBuddyAppFactory archived = new(TaxBuddyAppFactory.UnreachableDatabase, withMicrosoft: true, microsoftEnabled: false);
    private readonly TaxBuddyAppFactory both = new(TaxBuddyAppFactory.UnreachableDatabase, withMicrosoft: true);
    private readonly TaxBuddyAppFactory microsoftOnly = new(TaxBuddyAppFactory.UnreachableDatabase, withGoogle: false, withMicrosoft: true);

    [Fact]
    public async Task While_archived_Microsoft_stays_hidden_even_with_its_keys_stored()
    {
        var client = archived.CreateHttpsClient();
        var html = await client.GetHtmlAsync("/signin");

        Assert.Contains("Continue with Google", html);
        Assert.DoesNotContain("Microsoft", html);
        Assert.DoesNotContain("Microsoft", await client.GetHtmlAsync("/help"));
        Assert.Contains("You sign in with Google before the message is sent.", await client.GetHtmlAsync("/help"));
        var post = await client.PostAsync(AccountEndpoints.ExternalSignInPath, Form(Token(html), ("provider", "Microsoft"), ("returnUrl", "/")));
        Assert.Equal(HttpStatusCode.BadRequest, post.StatusCode);
    }

    [Fact]
    public async Task The_sign_in_page_offers_both_providers_with_protected_forms()
    {
        var html = await both.CreateHttpsClient().GetHtmlAsync("/signin?returnUrl=" + Uri.EscapeDataString("/accountants/x"));

        Assert.Contains("Continue with Google", html);
        Assert.Contains("Continue with Microsoft", html);
        Assert.Contains("""name="provider" value="Microsoft" """.Trim(), html);
        Assert.True(html.IndexOf("Continue with Google", StringComparison.Ordinal) < html.IndexOf("Continue with Microsoft", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Continue_with_Microsoft_sends_the_browser_to_Microsoft_with_the_site_callback()
    {
        var client = both.CreateHttpsClient();
        var token = Token(await client.GetHtmlAsync("/signin"));

        var response = await client.PostAsync(AccountEndpoints.ExternalSignInPath, Form(token, ("provider", "Microsoft"), ("returnUrl", "/accountants/x")));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var location = response.Headers.Location!.AbsoluteUri;
        Assert.StartsWith("https://login.microsoftonline.com/common/oauth2/v2.0/authorize", location);
        Assert.Contains("client_id=" + Uri.EscapeDataString(TaxBuddyAppFactory.MicrosoftClientId), location);
        Assert.Contains("redirect_uri=" + Uri.EscapeDataString("https://localhost/signin-microsoft"), location);
    }

    [Fact]
    public async Task Cancelling_at_Microsoft_returns_to_the_starting_page()
    {
        var client = both.CreateHttpsClient();
        var token = Token(await client.GetHtmlAsync("/signin"));
        var challenge = await client.PostAsync(AccountEndpoints.ExternalSignInPath, Form(token, ("provider", "Microsoft"), ("returnUrl", "/accountants/x?from=cancel")));
        var state = System.Web.HttpUtility.ParseQueryString(challenge.Headers.Location!.Query)["state"]!; // the client keeps the correlation cookie

        var response = await client.GetAsync($"/signin-microsoft?error=access_denied&state={Uri.EscapeDataString(state)}");

        Assert.Equal("/accountants/x?from=cancel", Target(response));
    }

    [Fact]
    public async Task With_Microsoft_only_the_page_offers_Microsoft_and_the_help_names_it()
    {
        var client = microsoftOnly.CreateHttpsClient();
        var html = await client.GetHtmlAsync("/signin");

        Assert.Contains("Continue with Microsoft", html);
        Assert.DoesNotContain("Continue with Google", html);
        Assert.DoesNotContain("isn't set up on this site yet", html);
        Assert.Contains("You sign in with Microsoft before the message is sent.", await client.GetHtmlAsync("/help"));
    }

    [Fact]
    public async Task With_both_the_help_names_both()
    {
        Assert.Contains("You sign in with Google or Microsoft before the message is sent.", await both.CreateHttpsClient().GetHtmlAsync("/help"));
    }

    public void Dispose()
    {
        archived.Dispose();
        both.Dispose();
        microsoftOnly.Dispose();
    }
}
