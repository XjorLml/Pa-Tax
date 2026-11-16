using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using TaxBuddy.TestSupport;
using TaxBuddy.Web.Domain;
using TaxBuddy.Web.Tests.Support;
using static TaxBuddy.Web.Tests.Support.FormPosts;

namespace TaxBuddy.Web.Tests;

/// <summary>F05 accountant profile editing through the whole app (PE01–PE07), on a throwaway schema.</summary>
public sealed partial class ProfileEditorTests(EmptyAppFixture app) : IClassFixture<EmptyAppFixture>
{
    private const string Editor = "/account/profile";
    private const string FormMarker = """<div class="checklist">""";

    [DatabaseFact]
    public async Task A_complete_profile_is_saved_listed_and_shown_publicly()
    {
        var client = await AccountantAsync("Andrea Villanueva");
        var page = await GetEditorAsync(client);
        Assert.Contains("Create your profile", page);
        Assert.Contains("Not listed yet", page);
        Assert.Contains("1 of 6</strong>", page); // only the display name, from Google
        Assert.Contains("""value="Andrea Villanueva" """.Trim(), page); // name from Google

        var saved = await SaveAsync(client, page, Complete(title: "Certified Public Accountant"));

        Assert.Equal(HttpStatusCode.Redirect, saved.StatusCode);
        Assert.Equal(Editor + "?saved=1", Target(saved));
        var after = await GetEditorAsync(client, Target(saved)!);
        Assert.Contains("Saved. Your profile is listed in the directory.", after);
        Assert.Contains("Everything needed to be listed is filled in.", after);
        var publicPath = ProfileLink().Match(after).Groups[1].Value;
        Assert.StartsWith("/accountants/", publicPath);

        var directory = await (await client.GetAsync("/")).ReadHtmlAsync();
        Assert.Contains("Andrea Villanueva", directory);
        var profile = await (await client.GetAsync(publicPath)).ReadHtmlAsync();
        Assert.Contains("Certified Public Accountant", profile);
        Assert.Contains("Monthly books for small firms.", profile);
        Assert.Contains("Senior Accountant", profile);
        Assert.Contains("andrea.contact@example.test", profile);
    }

    [DatabaseFact]
    public async Task An_unfinished_profile_is_saved_but_not_listed_and_names_what_is_missing()
    {
        var client = await AccountantAsync("Benjie Ramos");
        var page = await GetEditorAsync(client);

        var fields = Complete(title: "Payroll specialist");
        fields.Remove("Form.Services[0].Name");
        fields.Remove("Form.Services[0].Description");
        fields.Remove("Form.ConsentToShowContact");
        var saved = await SaveAsync(client, page, fields);

        var after = await GetEditorAsync(client, Target(saved)!);
        Assert.Contains("Saved. Not listed yet: add at least one service, agreement to show contact details.", after);
        Assert.Contains("Add: at least one service</a>", after);
        // As a visitor: a signed-in accountant sees their own name in the header.
        Assert.DoesNotContain("Benjie Ramos", await (await app.Factory.CreateHttpsClient().GetAsync("/?q=Benjie")).ReadHtmlAsync());
        await using var db = app.Schema.CreateContext();
        var stored = await db.AccountantProfiles.SingleAsync(p => p.DisplayName == "Benjie Ramos");
        Assert.NotNull(stored.OwnerAccountId);
        Assert.False(stored.IsListed);
    }

    [DatabaseFact]
    public async Task Invalid_input_saves_nothing_and_keeps_everything_typed()
    {
        var client = await AccountantAsync("Carla Mendoza");
        var page = await GetEditorAsync(client);
        var fields = Complete(title: new string('t', 101));
        fields["Form.Email"] = "carla-at-example";
        fields["Form.Experience[0].End"] = "2010-01";
        fields.Remove("Form.Experience[0].Current");

        var response = await SaveAsync(client, page, fields);
        var html = await response.ReadHtmlAsync();

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Contains("Fix these before saving", html);
        Assert.Contains("Professional title can be at most 100 characters (now 101).", html);
        Assert.Contains("Enter an email address like name@example.com.", html);
        Assert.Contains("Experience 1: the end month must be after the start month.", html);
        Assert.Contains("""value="carla-at-example" """.Trim(), html);
        Assert.Contains("Monthly books for small firms.", html);
        Assert.Matches("""id="f-email"[^>]*aria-invalid="true" """.Trim(), html);
        await using var db = app.Schema.CreateContext();
        Assert.False(await db.AccountantProfiles.AnyAsync(p => p.DisplayName == "Carla Mendoza"));
    }

    [DatabaseFact]
    public async Task Adding_and_removing_experience_rows_only_redraws_the_form()
    {
        var client = await AccountantAsync("Daniel Cruz");
        var page = await GetEditorAsync(client);

        var added = await PostAsync(client, page, Complete(title: "Auditor"), "add-experience");
        var addedHtml = await added.ReadHtmlAsync();
        Assert.Equal(HttpStatusCode.OK, added.StatusCode);
        Assert.Contains("""name="Form.Experience[1].Role" """.Trim(), addedHtml);
        Assert.Contains("""value="Auditor" """.Trim(), addedHtml); // typed text kept

        var fields = Complete(title: "Auditor");
        fields["Form.Experience[1].Role"] = "Junior Auditor";
        var removed = await PostAsync(client, addedHtml, fields, "remove-experience:0");
        var removedHtml = await removed.ReadHtmlAsync();
        Assert.Contains("""value="Junior Auditor" """.Trim(), removedHtml);
        Assert.DoesNotContain("Senior Accountant", removedHtml);
        Assert.DoesNotContain("""name="Form.Experience[1].Role" """.Trim(), removedHtml);

        await using var db = app.Schema.CreateContext();
        Assert.False(await db.AccountantProfiles.AnyAsync(p => p.DisplayName == "Daniel Cruz")); // nothing saved yet
    }

    [DatabaseFact]
    public async Task A_new_profile_with_no_experience_rows_can_add_its_first_row()
    {
        // Regression (browser check, 2026-11-16): a post with no experience fields bound the list as null.
        var client = await AccountantAsync("Mara Ong");
        var page = await GetEditorAsync(client);
        var fields = HiddenFields(page, FormMarker);
        fields["Form.DisplayName"] = "Mara Ong";
        fields["command"] = "add-experience";

        var added = await client.PostAsync(Editor, new FormUrlEncodedContent(fields));

        Assert.Equal(HttpStatusCode.OK, added.StatusCode);
        Assert.Contains("""name="Form.Experience[0].Role" """.Trim(), await added.ReadHtmlAsync());
    }

    [DatabaseFact]
    public async Task Editing_a_saved_profile_changes_the_public_profile()
    {
        var client = await AccountantAsync("Elaine Navarro");
        var first = await SaveAsync(client, await GetEditorAsync(client), Complete(title: "Tax adviser", withAudit: true));
        var page = await GetEditorAsync(client, Target(first)!);
        Assert.Contains("""value="Tax adviser" """.Trim(), page);

        var fields = Complete(title: "Senior tax adviser"); // Audit no longer offered
        var second = await SaveAsync(client, page, fields);

        Assert.Equal(HttpStatusCode.Redirect, second.StatusCode);
        await using var db = app.Schema.CreateContext();
        var stored = await db.AccountantProfiles.SingleAsync(p => p.DisplayName == "Elaine Navarro");
        Assert.Equal("Senior tax adviser", stored.Title);
        Assert.Equal(new[] { "Bookkeeping" }, stored.Services.Select(s => s.Name));
        Assert.Equal(2, stored.Version);
    }

    [DatabaseFact]
    public async Task A_save_from_an_older_tab_is_refused_and_keeps_its_edits_until_saved_again()
    {
        var client = await AccountantAsync("Francis Lim");
        await SaveAsync(client, await GetEditorAsync(client), Complete(title: "Bookkeeper"));
        var tabA = await GetEditorAsync(client);
        var tabB = await GetEditorAsync(client);

        await SaveAsync(client, tabA, Complete(title: "Title from tab A"));
        var stale = await SaveAsync(client, tabB, Complete(title: "Title from tab B"));
        var staleHtml = await stale.ReadHtmlAsync();

        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        Assert.Contains("This profile was changed in another tab or window", staleHtml);
        Assert.Contains("""value="Title from tab B" """.Trim(), staleHtml);
        await using (var db = app.Schema.CreateContext())
        {
            Assert.Equal("Title from tab A", (await db.AccountantProfiles.SingleAsync(p => p.DisplayName == "Francis Lim")).Title);
        }

        var again = await SaveAsync(client, staleHtml, Complete(title: "Title from tab B"));
        Assert.Equal(HttpStatusCode.Redirect, again.StatusCode);
        await using var check = app.Schema.CreateContext();
        Assert.Equal("Title from tab B", (await check.AccountantProfiles.SingleAsync(p => p.DisplayName == "Francis Lim")).Title);
    }

    [DatabaseFact]
    public async Task Visitors_sign_in_first_and_clients_have_no_profile()
    {
        var visitor = app.Factory.CreateHttpsClient();
        var redirect = await visitor.GetAsync(Editor);
        Assert.Equal("/signin?returnUrl=%2Faccount%2Fprofile", Target(redirect));

        var client = app.Factory.CreateHttpsClient();
        await TestAccounts.RegisterAsync(client, AccountRole.Client, "Grace Tan", $"{Guid.NewGuid():N}@example.test");
        var response = await client.GetAsync(Editor);
        var html = await response.ReadHtmlAsync();
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Contains("Only accountants have a public profile", html);
        Assert.DoesNotContain(FormMarker, html);
        Assert.DoesNotMatch(MyProfileLink(), await (await client.GetAsync("/account")).ReadHtmlAsync());
    }

    [DatabaseFact]
    public async Task Each_accountant_edits_only_their_own_profile()
    {
        var hannah = await AccountantAsync("Hannah de Guzman");
        var ivan = await AccountantAsync("Ivan Reyes");
        await SaveAsync(hannah, await GetEditorAsync(hannah), Complete(title: "Hannah's title"));

        var ivanPage = await GetEditorAsync(ivan);
        Assert.DoesNotContain("Hannah", ivanPage);
        await SaveAsync(ivan, ivanPage, Complete(title: "Ivan's title"));

        await using var db = app.Schema.CreateContext();
        Assert.Equal("Hannah's title", (await db.AccountantProfiles.SingleAsync(p => p.DisplayName == "Hannah de Guzman")).Title);
        Assert.Equal("Ivan's title", (await db.AccountantProfiles.SingleAsync(p => p.DisplayName == "Ivan Reyes")).Title);
    }

    [DatabaseFact]
    public async Task A_post_without_the_antiforgery_token_is_refused()
    {
        var client = await AccountantAsync("Jun Santos");
        var fields = HiddenFields(await GetEditorAsync(client), FormMarker);
        fields.Remove("__RequestVerificationToken");
        foreach (var (name, value) in Complete(title: "Forged")) fields[name] = value;
        fields["command"] = "save";

        var response = await client.PostAsync(Editor, new FormUrlEncodedContent(fields));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await using var db = app.Schema.CreateContext();
        Assert.False(await db.AccountantProfiles.AnyAsync(p => p.DisplayName == "Jun Santos"));
    }

    [DatabaseFact]
    public async Task An_accountant_registering_from_the_home_page_lands_on_the_editor_and_sees_My_profile()
    {
        var client = app.Factory.CreateHttpsClient();
        await client.GetAsync(TestExternalSignIn.Address(Guid.NewGuid().ToString("N"), "kim@example.test", "Kim Lee"));
        await client.GetAsync("/account/signin/callback?returnUrl=%2F");
        var register = await (await client.GetAsync("/register?returnUrl=%2F")).ReadHtmlAsync();
        var fields = HiddenFields(register, "How will you use Tax Buddy?");
        fields["Form.Role"] = "accountant";
        fields["Form.Agree"] = "true";

        var registered = await client.PostAsync("/register", new FormUrlEncodedContent(fields));

        Assert.Equal(Editor, Target(registered));
        Assert.Matches(MyProfileLink(), await (await client.GetAsync("/account")).ReadHtmlAsync());
    }

    [DatabaseFact]
    public async Task When_the_database_is_unavailable_the_editor_says_so_and_a_save_keeps_the_input()
    {
        // Sign in on the working database, then use the same session cookie on a host whose database is unreachable.
        // Both hosts share the test machine's data-protection keys, so the cookie is valid on both.
        var session = await TestAccounts.RegisterAsync(app.Factory.CreateHttpsClient(), AccountRole.Accountant, "Lea Cruz",
            $"{Guid.NewGuid():N}@example.test");
        await using var broken = new TaxBuddyAppFactory(TaxBuddyAppFactory.UnreachableDatabase);
        var client = broken.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost"),
            HandleCookies = false,
        });

        var load = await client.SendAsync(WithCookies(HttpMethod.Get, Editor, session));
        Assert.Equal(HttpStatusCode.ServiceUnavailable, load.StatusCode);
        Assert.Contains("We couldn't load your profile", await load.ReadHtmlAsync());

        var terms = await client.SendAsync(WithCookies(HttpMethod.Get, "/terms", session));
        var antiforgery = Cookies(terms).Single(c => c.StartsWith(".AspNetCore.Antiforgery.", StringComparison.Ordinal));
        var fields = Complete(title: "Kept title");
        fields["Form.DisplayName"] = "Lea Cruz";
        fields["__RequestVerificationToken"] = Token(await terms.ReadHtmlAsync());
        fields["_handler"] = "profile";
        fields["Form.Version"] = "0";
        fields["command"] = "save";
        var post = WithCookies(HttpMethod.Post, Editor, session, antiforgery);
        post.Content = new FormUrlEncodedContent(fields);
        var save = await client.SendAsync(post);
        var html = await save.ReadHtmlAsync();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, save.StatusCode);
        Assert.Contains("Not saved: we couldn't reach the server. Your changes are still here.", html);
        Assert.Contains("""value="Kept title" """.Trim(), html);
    }

    private static HttpRequestMessage WithCookies(HttpMethod method, string address, params string[] cookies)
    {
        var request = new HttpRequestMessage(method, address);
        request.Headers.Add("Cookie", string.Join("; ", cookies));
        return request;
    }

    private async Task<HttpClient> AccountantAsync(string name)
    {
        var client = app.Factory.CreateHttpsClient();
        await TestAccounts.RegisterAsync(client, AccountRole.Accountant, name, $"{Guid.NewGuid():N}@example.test");
        return client;
    }

    private static async Task<string> GetEditorAsync(HttpClient client, string address = Editor)
    {
        var response = await client.GetAsync(address);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.ReadHtmlAsync();
    }

    private static Task<HttpResponseMessage> SaveAsync(HttpClient client, string page, Dictionary<string, string> fields) =>
        PostAsync(client, page, fields, "save");

    /// <summary>Posts the editor as the browser would: its hidden fields (token, form name, version) plus the given values.</summary>
    private static Task<HttpResponseMessage> PostAsync(HttpClient client, string page, Dictionary<string, string> values, string command)
    {
        var fields = HiddenFields(page, FormMarker);
        // A browser also posts the visible name field, prefilled from Google or the saved profile.
        fields["Form.DisplayName"] = DisplayNameInput().Match(page).Groups[1].Value;
        foreach (var (name, value) in values)
        {
            fields[name] = value;
        }

        fields["command"] = command;
        return client.PostAsync(Editor, new FormUrlEncodedContent(fields));
    }

    /// <summary>A complete profile as the form posts it, with one service row (two with Audit).</summary>
    private static Dictionary<string, string> Complete(string title, bool withAudit = false)
    {
        var fields = new Dictionary<string, string>
        {
            ["Form.Title"] = title,
            ["Form.City"] = "Makati City",
            ["Form.Summary"] = "Clean books, filed on time.",
            ["Form.Email"] = "andrea.contact@example.test",
            ["Form.Phone"] = "+63 900 000 0000",
            ["Form.Website"] = "example.com",
            ["Form.ConsentToShowContact"] = "true",
            ["Form.Services[0].Name"] = "Bookkeeping",
            ["Form.Services[0].Description"] = "Monthly books for small firms.",
            ["Form.Experience[0].Role"] = "Senior Accountant",
            ["Form.Experience[0].Organization"] = "Synthetic & Co.",
            ["Form.Experience[0].Start"] = "2018-03",
            ["Form.Experience[0].Current"] = "true",
        };
        if (withAudit)
        {
            fields["Form.Services[1].Name"] = "Audit";
        }

        return fields;
    }

    [GeneratedRegex("""<a href="(/accountants/[0-9a-f-]+)">View public profile</a>""")]
    private static partial Regex ProfileLink();

    [GeneratedRegex("""name="Form.DisplayName" type="text" value="([^"]*)" """)]
    private static partial Regex DisplayNameInput();

    [GeneratedRegex("""<a class="mi" href="/account/profile">""")]
    private static partial Regex MyProfileLink();
}
