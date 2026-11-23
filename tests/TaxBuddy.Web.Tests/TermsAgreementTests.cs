using System.Net;
using Microsoft.EntityFrameworkCore;
using TaxBuddy.TestSupport;
using TaxBuddy.Web.Account;
using TaxBuddy.Web.Domain;
using TaxBuddy.Web.Tests.Support;
using static TaxBuddy.Web.Tests.Support.FormPosts;

namespace TaxBuddy.Web.Tests;

/// <summary>
/// F11 through the whole app: agreeing again to a new version of the terms (LP05–LP08) and a closed account (LP13). An
/// older agreement is simulated by writing an old version into <c>users</c> and signing in again, which issues a session
/// with that version, as a tester who registered before F11 has.
/// </summary>
public sealed class TermsAgreementTests(EmptyAppFixture app) : IClassFixture<EmptyAppFixture>
{
    private const string OldVersion = "draft-2026-11-16";

    [DatabaseFact]
    public async Task A_new_registration_stores_the_current_version_and_is_never_asked_again()
    {
        var person = await RegisterAsync(AccountRole.Client, "Rina Cruz");

        Assert.Equal(HttpStatusCode.OK, (await person.Client.GetAsync("/")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await person.Client.GetAsync("/account")).StatusCode);
        await using var db = app.Schema.CreateContext();
        Assert.Equal(LegalText.CurrentVersion, (await db.Users.SingleAsync(u => u.Id == person.Id)).AgreedTermsVersion);
    }

    [DatabaseFact]
    public async Task Someone_who_agreed_to_an_older_version_is_sent_to_agree_before_other_pages()
    {
        var person = await RegisterAsync(AccountRole.Client, "Rina Cruz");
        await SetVersionAsync(person.Id, OldVersion);
        await SignInAgainAsync(person);

        var chats = await person.Client.GetAsync("/chats");
        var home = await person.Client.GetAsync("/?q=tax");

        Assert.Equal(HttpStatusCode.Redirect, chats.StatusCode);
        Assert.Equal(TermsAgreement.AgreePageAddress("/chats"), Target(chats));
        Assert.Equal(TermsAgreement.AgreePageAddress("/?q=tax"), Target(home));
        foreach (var open in new[] { "/terms", "/privacy", TermsAgreement.AgreePageAddress("/chats"), "/no-such-page" })
        {
            var response = await person.Client.GetAsync(open);
            Assert.NotEqual(HttpStatusCode.Redirect, response.StatusCode);
        }

        var agree = await person.Client.GetHtmlAsync(TermsAgreement.AgreePageAddress("/chats"));
        Assert.Contains("We've updated our terms", agree);
        Assert.Contains($"changed on {LegalText.UpdatedOn}", agree);
        Assert.Contains("""I agree to the <a href="/terms">Terms of use</a> and have read the <a href="/privacy">Privacy policy</a>.""", agree);
        Assert.Contains($"""action="{AccountEndpoints.SignOutPath}" """.Trim(), agree);
    }

    [DatabaseFact]
    public async Task Agreeing_stores_the_version_renews_the_session_and_returns_to_the_page()
    {
        var person = await RegisterAsync(AccountRole.Client, "Rina Cruz");
        await SetVersionAsync(person.Id, OldVersion);
        await SignInAgainAsync(person);
        var page = await person.Client.GetHtmlAsync(TermsAgreement.AgreePageAddress("/chats"));

        var fields = HiddenFields(page, "We've updated our terms");
        fields["Form.Agree"] = "true";
        var agreed = await person.Client.PostAsync(TermsAgreement.AgreePath, new FormUrlEncodedContent(fields));

        Assert.Equal(HttpStatusCode.Redirect, agreed.StatusCode);
        Assert.Equal("/chats", Target(agreed));
        Assert.Contains(Cookies(agreed), c => c.StartsWith(".AspNetCore.Identity.Application=", StringComparison.Ordinal));
        Assert.Equal(HttpStatusCode.OK, (await person.Client.GetAsync("/chats")).StatusCode);
        await using var db = app.Schema.CreateContext();
        var user = await db.Users.SingleAsync(u => u.Id == person.Id);
        Assert.Equal(LegalText.CurrentVersion, user.AgreedTermsVersion);
        Assert.InRange(user.AgreedToTermsAt, DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow.AddMinutes(1));
    }

    [DatabaseFact]
    public async Task Continuing_without_the_tick_stores_nothing_and_keeps_asking()
    {
        var person = await RegisterAsync(AccountRole.Accountant, "Nina Ocampo");
        await SetVersionAsync(person.Id, OldVersion);
        await SignInAgainAsync(person);
        var page = await person.Client.GetHtmlAsync(TermsAgreement.AgreePageAddress("/account"));

        var response = await person.Client.PostAsync(TermsAgreement.AgreePath, new FormUrlEncodedContent(HiddenFields(page, "We've updated our terms")));
        var html = await response.ReadHtmlAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Tick the box to agree to the Terms of use.", html);
        Assert.Equal(HttpStatusCode.Redirect, (await person.Client.GetAsync("/account")).StatusCode);
        await using var db = app.Schema.CreateContext();
        Assert.Equal(OldVersion, (await db.Users.SingleAsync(u => u.Id == person.Id)).AgreedTermsVersion);
    }

    [DatabaseFact]
    public async Task A_visitor_on_the_agree_page_is_asked_to_sign_in()
    {
        var response = await app.Factory.CreateHttpsClient().GetAsync(TermsAgreement.AgreePath);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(AccountEndpoints.SignInPageAddress(TermsAgreement.AgreePageAddress("/")), Target(response));
    }

    [DatabaseFact]
    public async Task An_account_closed_by_hand_cannot_sign_in_again()
    {
        // RELEASE "Acting on a report", step 7: lockout_end far in the future.
        var person = await RegisterAsync(AccountRole.Client, "Rina Cruz");
        await using (var db = app.Schema.CreateContext())
        {
            var user = await db.Users.SingleAsync(u => u.Id == person.Id);
            Assert.True(user.LockoutEnabled);
            user.LockoutEnd = new DateTimeOffset(9999, 12, 31, 0, 0, 0, TimeSpan.Zero);
            await db.SaveChangesAsync();
        }

        var elsewhere = app.Factory.CreateHttpsClient();
        Assert.Equal(HttpStatusCode.NoContent, (await elsewhere.GetAsync(TestExternalSignIn.Address(person.Key, person.Email, "Rina Cruz"))).StatusCode);
        var callback = await elsewhere.GetAsync(AccountEndpoints.CallbackPath + "?returnUrl=%2Fchats");

        Assert.Equal(AccountEndpoints.SignInPageAddress("/chats", "failed"), Target(callback));
        Assert.DoesNotContain(Cookies(callback), c => c.StartsWith(".AspNetCore.Identity.Application=", StringComparison.Ordinal));
    }

    private sealed record Person(HttpClient Client, Guid Id, string Key, string Email, string Name);

    private async Task<Person> RegisterAsync(AccountRole role, string name)
    {
        var client = app.Factory.CreateHttpsClient();
        var key = Guid.NewGuid().ToString("N");
        var email = $"{key}@example.test";
        Assert.Equal(HttpStatusCode.NoContent, (await client.GetAsync(TestExternalSignIn.Address(key, email, name))).StatusCode);
        await client.GetAsync(AccountEndpoints.CallbackPath + "?returnUrl=%2Fterms");
        var page = await client.GetHtmlAsync(AccountEndpoints.RegisterPageAddress("/terms"));

        var fields = HiddenFields(page, "How will you use Tax Buddy?");
        fields["Form.Role"] = AccountRoles.ValueOf(role);
        fields["Form.Agree"] = "true";
        Assert.Equal("/terms", Target(await client.PostAsync(AccountEndpoints.RegisterPath, new FormUrlEncodedContent(fields))));

        await using var db = app.Schema.CreateContext();
        return new Person(client, (await db.Users.SingleAsync(u => u.Email == email)).Id, key, email, name);
    }

    /// <summary>A returning sign-in: the session is issued again from the database, with whatever version it holds.</summary>
    private static async Task SignInAgainAsync(Person person)
    {
        Assert.Equal(HttpStatusCode.NoContent, (await person.Client.GetAsync(TestExternalSignIn.Address(person.Key, person.Email, person.Name))).StatusCode);
        var callback = await person.Client.GetAsync(AccountEndpoints.CallbackPath + "?returnUrl=%2Fchats");
        Assert.Equal("/chats", Target(callback));
    }

    private async Task SetVersionAsync(Guid id, string version)
    {
        await using var db = app.Schema.CreateContext();
        var user = await db.Users.SingleAsync(u => u.Id == id);
        user.AgreedTermsVersion = version;
        await db.SaveChangesAsync();
    }
}
