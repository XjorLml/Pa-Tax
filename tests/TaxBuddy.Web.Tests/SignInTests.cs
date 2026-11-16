using System.Net;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TaxBuddy.TestSupport;
using TaxBuddy.Web.Account;
using TaxBuddy.Web.Domain;
using TaxBuddy.Web.Tests.Support;

namespace TaxBuddy.Web.Tests;

/// <summary>
/// F04 Google sign-in and registration through the whole app (GS01–GS08, GS10). The test sign-in stands in for
/// Google's round trip; cancel and error use Google's real callback address with a real state value.
/// </summary>
public sealed partial class SignInTests(EmptyAppFixture app) : IClassFixture<EmptyAppFixture>
{
    private const string StartPage = "/accountants/0193a6c0-0000-7000-8000-000000000001?from=test";

    [DatabaseFact]
    public async Task First_Google_sign_in_registers_one_role_and_returns_to_the_starting_page()
    {
        var client = app.Factory.CreateHttpsClient();
        var key = NewKey();

        var callback = await ArriveFromGoogleAsync(client, key, "ana.reyes@example.test", "Ana Reyes");
        Assert.Equal(HttpStatusCode.Redirect, callback.StatusCode);
        Assert.Equal(AccountEndpoints.RegisterPageAddress(StartPage), Target(callback));

        var form = await client.GetAsync(AccountEndpoints.RegisterPageAddress(StartPage));
        var html = await form.ReadHtmlAsync();
        Assert.Equal(HttpStatusCode.OK, form.StatusCode);
        Assert.Contains("How will you use Tax Buddy?", html);
        Assert.Contains("ana.reyes@example.test", html);
        Assert.Contains("""<a href="/terms">Terms of use</a>""", html);

        var registered = await PostRegisterAsync(client, html, role: "client", agree: true);
        Assert.Equal(HttpStatusCode.Redirect, registered.StatusCode);
        Assert.Equal(StartPage, Target(registered));
        var cookies = registered.Headers.GetValues("Set-Cookie").Select(c => c[..c.IndexOf('=')]).ToList();
        Assert.Contains(".AspNetCore.Identity.Application", cookies);

        var home = await client.GetAsync("/");
        var homeHtml = await home.ReadHtmlAsync();
        Assert.Contains("Ana Reyes", homeHtml);
        Assert.Contains("· Client", homeHtml);
        Assert.Contains(">Sign out</button>", homeHtml);

        await using var db = app.Schema.CreateContext();
        var user = await db.Users.SingleAsync(u => u.Email == "ana.reyes@example.test");
        Assert.Equal(AccountRole.Client, user.Role);
        Assert.Equal("Ana Reyes", user.DisplayName);
        Assert.Equal(LegalText.CurrentVersion, user.AgreedTermsVersion);
        Assert.InRange(user.AgreedToTermsAt, DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow.AddMinutes(1));
        Assert.Null(user.PasswordHash);
        var login = await db.UserLogins.SingleAsync(l => l.UserId == user.Id);
        Assert.Equal(("Google", key), (login.LoginProvider, login.ProviderKey));
    }

    [DatabaseFact]
    public async Task Registering_as_an_accountant_stores_that_role_and_lists_nothing()
    {
        var client = app.Factory.CreateHttpsClient();
        await ArriveFromGoogleAsync(client, NewKey(), "ben.cruz@example.test", "Ben Cruz");
        var html = await (await client.GetAsync(AccountEndpoints.RegisterPageAddress("/"))).ReadHtmlAsync();

        var registered = await PostRegisterAsync(client, html, role: "accountant", agree: true);

        Assert.Equal("/", Target(registered));
        await using var db = app.Schema.CreateContext();
        Assert.Equal(AccountRole.Accountant, (await db.Users.SingleAsync(u => u.Email == "ben.cruz@example.test")).Role);
        Assert.Empty(await db.AccountantProfiles.ToListAsync()); // BR01: nothing listed until R05
        Assert.Contains("· Accountant", await (await client.GetAsync("/terms")).ReadHtmlAsync());
    }

    [DatabaseTheory]
    [InlineData("client", false, "Tick the box to agree")]
    [InlineData("", true, "Choose whether you're looking for an accountant")]
    [InlineData("admin", true, "Choose whether you're looking for an accountant")]
    public async Task Registration_without_agreement_or_a_valid_role_keeps_the_choice_and_creates_nothing(string role, bool agree, string error)
    {
        var client = app.Factory.CreateHttpsClient();
        var email = $"{NewKey()}@example.test";
        await ArriveFromGoogleAsync(client, NewKey(), email, "Cara Lim");
        var html = await (await client.GetAsync(AccountEndpoints.RegisterPageAddress(StartPage))).ReadHtmlAsync();

        var response = await PostRegisterAsync(client, html, role, agree);
        var again = await response.ReadHtmlAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(error, again);
        if (role == "client")
        {
            Assert.Matches("""value="client" checked""", again);
        }

        Assert.DoesNotContain(response.Headers.TryGetValues("Set-Cookie", out var cookies) ? cookies : [],
            c => c.StartsWith(".AspNetCore.Identity.Application=", StringComparison.Ordinal));
        await using var db = app.Schema.CreateContext();
        Assert.False(await db.Users.AnyAsync(u => u.Email == email));
    }

    [DatabaseFact]
    public async Task A_returning_person_skips_registration_and_goes_back_to_the_starting_page()
    {
        var key = NewKey();
        var first = app.Factory.CreateHttpsClient();
        await ArriveFromGoogleAsync(first, key, "dina@example.test", "Dina Tan");
        await PostRegisterAsync(first, await (await first.GetAsync(AccountEndpoints.RegisterPageAddress("/"))).ReadHtmlAsync(), "client", agree: true);

        var later = app.Factory.CreateHttpsClient();
        var callback = await ArriveFromGoogleAsync(later, key, "dina@example.test", "Dina Tan");

        Assert.Equal(StartPage, Target(callback));
        Assert.Contains("Dina Tan", await (await later.GetAsync("/terms")).ReadHtmlAsync());
        await using var db = app.Schema.CreateContext();
        Assert.Equal(1, await db.Users.CountAsync(u => u.Email == "dina@example.test"));
    }

    [DatabaseFact]
    public async Task Registering_the_same_Google_account_in_two_tabs_keeps_one_account()
    {
        var key = NewKey();
        var tab1 = app.Factory.CreateHttpsClient();
        var tab2 = app.Factory.CreateHttpsClient();
        await ArriveFromGoogleAsync(tab1, key, "eli@example.test", "Eli Go");
        await ArriveFromGoogleAsync(tab2, key, "eli@example.test", "Eli Go");
        var page1 = await (await tab1.GetAsync(AccountEndpoints.RegisterPageAddress("/"))).ReadHtmlAsync();
        var page2 = await (await tab2.GetAsync(AccountEndpoints.RegisterPageAddress("/"))).ReadHtmlAsync();

        await PostRegisterAsync(tab1, page1, "client", agree: true);
        var second = await PostRegisterAsync(tab2, page2, "accountant", agree: true);

        // The second tab signs in to the account the first one created; its different choice is not applied.
        Assert.Equal("/", Target(second));
        Assert.Contains("Eli Go", await (await tab2.GetAsync("/terms")).ReadHtmlAsync());
        await using var db = app.Schema.CreateContext();
        var user = await db.Users.SingleAsync(u => u.Email == "eli@example.test");
        Assert.Equal(AccountRole.Client, user.Role);
    }

    [DatabaseFact]
    public async Task Sign_out_ends_the_session_in_this_browser()
    {
        var client = app.Factory.CreateHttpsClient();
        await ArriveFromGoogleAsync(client, NewKey(), "fe@example.test", "Fe Santos");
        await PostRegisterAsync(client, await (await client.GetAsync(AccountEndpoints.RegisterPageAddress("/"))).ReadHtmlAsync(), "client", agree: true);
        var page = await (await client.GetAsync("/terms")).ReadHtmlAsync();

        var signedOut = await client.PostAsync(AccountEndpoints.SignOutPath, Form(Token(page)));

        Assert.Equal(HttpStatusCode.Redirect, signedOut.StatusCode);
        Assert.Equal("/", Target(signedOut));
        var after = await (await client.GetAsync("/terms")).ReadHtmlAsync();
        Assert.DoesNotContain("Fe Santos", after);
        Assert.Contains(">Sign in</a>", after);
    }

    [DatabaseFact]
    public async Task Off_site_return_addresses_go_home()
    {
        var key = NewKey();
        var client = app.Factory.CreateHttpsClient();
        await client.GetAsync(TestExternalSignIn.Address(key, "gil@example.test", "Gil Uy"));

        var callback = await client.GetAsync(AccountEndpoints.CallbackPath + "?returnUrl=" + Uri.EscapeDataString("//evil.example/"));

        Assert.Equal(AccountEndpoints.RegisterPageAddress("/"), Target(callback));
        var html = await (await client.GetAsync(AccountEndpoints.RegisterPath + "?returnUrl=https%3A%2F%2Fevil.example")).ReadHtmlAsync();
        Assert.Contains("""name="Form.ReturnUrl" value="/" """.Trim(), html);
    }

    [DatabaseFact]
    public async Task Personal_details_from_Google_are_never_logged()
    {
        var client = app.Factory.CreateHttpsClient();
        await ArriveFromGoogleAsync(client, NewKey(), "private.person@example.test", "Private Person");
        await PostRegisterAsync(client, await (await client.GetAsync(AccountEndpoints.RegisterPageAddress("/"))).ReadHtmlAsync(), "accountant", agree: true);

        Assert.NotEmpty(app.Factory.Logs.Messages);
        Assert.DoesNotContain(app.Factory.Logs.Messages, m => m.Contains("private.person", StringComparison.OrdinalIgnoreCase)
                                                            || m.Contains("Private Person", StringComparison.Ordinal));
    }

    private static string NewKey() => Guid.NewGuid().ToString("N");

    /// <summary>A redirect's path and query: endpoints send relative addresses, Blazor pages absolute ones.</summary>
    internal static string? Target(HttpResponseMessage response) =>
        response.Headers.Location is { } location ? (location.IsAbsoluteUri ? location.PathAndQuery : location.OriginalString) : null;

    /// <summary>The browser comes back from Google: the test sign-in, then the site's callback (as Google's handler redirects).</summary>
    private static async Task<HttpResponseMessage> ArriveFromGoogleAsync(HttpClient client, string key, string email, string name)
    {
        var signedIn = await client.GetAsync(TestExternalSignIn.Address(key, email, name));
        Assert.Equal(HttpStatusCode.NoContent, signedIn.StatusCode);
        return await client.GetAsync(AccountEndpoints.CallbackPath + "?returnUrl=" + Uri.EscapeDataString(StartPage));
    }

    private static Task<HttpResponseMessage> PostRegisterAsync(HttpClient client, string registerPage, string role, bool agree)
    {
        var fields = HiddenFields(registerPage);
        fields["Form.Role"] = role;
        if (agree)
        {
            fields["Form.Agree"] = "true";
        }

        // The registration form posts to its own page (SDD named forms); the header's theme form is a different one.
        return client.PostAsync(AccountEndpoints.RegisterPath, new FormUrlEncodedContent(fields));
    }

    internal static string Token(string html) =>
        TokenPattern().Match(html) is { Success: true } m ? m.Groups[1].Value : throw new InvalidOperationException("No antiforgery token on the page.");

    internal static FormUrlEncodedContent Form(string token, params (string Name, string Value)[] fields) =>
        new(new Dictionary<string, string>(fields.Select(f => KeyValuePair.Create(f.Name, f.Value)))
        {
            ["__RequestVerificationToken"] = token,
        });

    /// <summary>The registration form's hidden fields: antiforgery token, Blazor's form name and the return address.</summary>
    private static Dictionary<string, string> HiddenFields(string html)
    {
        var form = html[html.IndexOf("""<form method="post" """, html.IndexOf("How will you use Tax Buddy?", StringComparison.Ordinal), StringComparison.Ordinal)..];
        form = form[..form.IndexOf("</form>", StringComparison.Ordinal)];
        return HiddenPattern().Matches(form).ToDictionary(m => m.Groups[1].Value, m => m.Groups[2].Value);
    }

    [GeneratedRegex("""name="__RequestVerificationToken" value="([^"]+)" """)]
    private static partial Regex TokenPattern();

    [GeneratedRegex("""<input type="hidden" name="([^"]+)" value="([^"]*)" """)]
    private static partial Regex HiddenPattern();
}

/// <summary>F04 checks that need no database: the sign-in page, the challenge, cancel and failure, legal pages.</summary>
public sealed class SignInWithoutDatabaseTests : IDisposable
{
    private static string? Target(HttpResponseMessage response) => SignInTests.Target(response);

    private readonly TaxBuddyAppFactory factory = new(TaxBuddyAppFactory.UnreachableDatabase);
    private readonly TaxBuddyAppFactory withoutGoogle = new(TaxBuddyAppFactory.UnreachableDatabase, withGoogle: false);

    [Fact]
    public async Task Sign_in_page_offers_Google_with_a_protected_form_and_keeps_the_return_address()
    {
        var html = await factory.CreateHttpsClient().GetHtmlAsync("/signin?returnUrl=" + Uri.EscapeDataString("/accountants/x"));

        Assert.Contains("Welcome to Tax Buddy", html);
        Assert.Contains($"""<form method="post" action="{AccountEndpoints.ExternalSignInPath}">""", html);
        Assert.Contains("""name="provider" value="Google" """.Trim(), html);
        Assert.Contains("""name="returnUrl" value="/accountants/x" """.Trim(), html);
        Assert.Contains("Continue with Google", html);
        Assert.Contains("name=\"__RequestVerificationToken\"", html);
    }

    [Fact]
    public async Task Continue_with_Google_sends_the_browser_to_Google_with_the_site_callback()
    {
        var client = factory.CreateHttpsClient();
        var token = SignInTests.Token(await client.GetHtmlAsync("/signin"));

        var response = await client.PostAsync(AccountEndpoints.ExternalSignInPath,
            SignInTests.Form(token, ("provider", "Google"), ("returnUrl", "/accountants/x")));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var location = response.Headers.Location!.AbsoluteUri;
        Assert.StartsWith("https://accounts.google.com/", location);
        Assert.Contains("client_id=" + Uri.EscapeDataString(TaxBuddyAppFactory.GoogleClientId), location);
        Assert.Contains("redirect_uri=" + Uri.EscapeDataString("https://localhost/signin-google"), location);
        Assert.Contains("scope=openid%20profile%20email", location);
    }

    [Theory]
    [InlineData(null, "Google")]
    [InlineData("token", "Facebook")]
    [InlineData("token", "")]
    public async Task A_forged_or_unknown_sign_in_request_is_refused(string? token, string provider)
    {
        var client = factory.CreateHttpsClient();
        var fields = new List<(string, string)> { ("provider", provider), ("returnUrl", "/") };
        var content = token is null
            ? new FormUrlEncodedContent(fields.ToDictionary(f => f.Item1, f => f.Item2))
            : SignInTests.Form(SignInTests.Token(await client.GetHtmlAsync("/signin")), [.. fields]);

        var response = await client.PostAsync(AccountEndpoints.ExternalSignInPath, content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Cancelling_at_Google_returns_to_the_starting_page_not_signed_in()
    {
        var (client, state) = await StartGoogleSignInAsync("/accountants/x?from=cancel");

        var response = await client.GetAsync($"/signin-google?error=access_denied&state={Uri.EscapeDataString(state)}");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/accountants/x?from=cancel", Target(response));
        Assert.DoesNotContain(response.Headers.TryGetValues("Set-Cookie", out var c) ? c : [],
            v => v.StartsWith(".AspNetCore.Identity.Application=", StringComparison.Ordinal));
    }

    [Fact]
    public async Task A_Google_error_shows_the_sign_in_page_with_a_retry()
    {
        var (client, state) = await StartGoogleSignInAsync("/accountants/x");

        var response = await client.GetAsync($"/signin-google?error=server_error&state={Uri.EscapeDataString(state)}");

        Assert.Equal(AccountEndpoints.SignInPageAddress("/accountants/x", "failed"), Target(response));
        var page = await client.GetHtmlAsync(Target(response)!);
        Assert.Contains("We couldn't sign you in with Google", page);
        Assert.Contains("Continue with Google", page);
        Assert.Contains("External sign-in failed:", string.Join("\n", factory.Logs.Messages));
    }

    [Fact]
    public async Task A_callback_with_no_sign_in_in_progress_goes_to_the_sign_in_page()
    {
        var client = factory.CreateHttpsClient();

        var callback = await client.GetAsync(AccountEndpoints.CallbackPath + "?returnUrl=%2Fterms");
        var register = await client.GetAsync(AccountEndpoints.RegisterPageAddress("/terms"));

        Assert.Equal(AccountEndpoints.SignInPageAddress("/terms", "failed"), Target(callback));
        Assert.Equal(HttpStatusCode.Redirect, register.StatusCode);
        Assert.Equal(AccountEndpoints.SignInPageAddress("/terms"), Target(register));
    }

    [Fact]
    public async Task Without_Google_configured_the_page_says_so_and_offers_no_button()
    {
        var client = withoutGoogle.CreateHttpsClient();
        var html = await client.GetHtmlAsync("/signin");

        Assert.Contains("Google sign-in isn't set up on this site yet.", html);
        Assert.DoesNotContain("Continue with Google", html);
        var token = SignInTests.Token(html); // the theme switch's form still has one
        var response = await client.PostAsync(AccountEndpoints.ExternalSignInPath, SignInTests.Form(token, ("provider", "Google"), ("returnUrl", "/")));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("/terms", "Terms of use")]
    [InlineData("/privacy", "Privacy")]
    public async Task Legal_pages_load_signed_out_and_say_they_are_drafts(string path, string heading)
    {
        var response = await factory.CreateHttpsClient().GetAsync(path);
        var html = await response.ReadHtmlAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains($">{heading}</h1>", html);
        Assert.Contains("Draft, not reviewed legal text.", html);
        Assert.Contains(LegalText.CurrentVersion, html);
    }

    [Fact]
    public async Task Every_page_links_to_the_terms_and_privacy_policy_and_offers_sign_in()
    {
        var html = await (await factory.CreateHttpsClient().GetAsync("/no-such-page")).ReadHtmlAsync();

        Assert.Matches("""<a href="/terms"[^>]*>Terms of use</a>""", html);
        Assert.Matches("""<a href="/privacy"[^>]*>Privacy</a>""", html);
        Assert.Contains("""href="/signin?returnUrl=%2Fno-such-page">Sign in</a>""", html);
    }

    public void Dispose()
    {
        factory.Dispose();
        withoutGoogle.Dispose();
    }

    /// <summary>Starts a real challenge, so the browser holds Google's correlation cookie and the state value.</summary>
    private async Task<(HttpClient Client, string State)> StartGoogleSignInAsync(string returnUrl)
    {
        var client = factory.CreateHttpsClient();
        var token = SignInTests.Token(await client.GetHtmlAsync("/signin"));
        var challenge = await client.PostAsync(AccountEndpoints.ExternalSignInPath,
            SignInTests.Form(token, ("provider", "Google"), ("returnUrl", returnUrl)));
        var query = System.Web.HttpUtility.ParseQueryString(challenge.Headers.Location!.Query);
        return (client, query["state"]!);
    }
}
