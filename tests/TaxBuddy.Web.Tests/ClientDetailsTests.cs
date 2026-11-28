using System.Net;
using Microsoft.EntityFrameworkCore;
using TaxBuddy.TestSupport;
using TaxBuddy.Web.Domain;
using TaxBuddy.Web.Tests.Support;
using static TaxBuddy.Web.Tests.Support.FormPosts;

namespace TaxBuddy.Web.Tests;

/// <summary>
/// F17 through the whole app (PRD R25; CD01–CD04, CD06, CD07): a client's details on Account, edited in place with a
/// named form, and what the accountants they have a service with see.
/// </summary>
public sealed class ClientDetailsTests(EmptyAppFixture app) : IClassFixture<EmptyAppFixture>
{
    private const string EditPage = "/account?edit=details";

    [DatabaseFact]
    public async Task A_client_sees_their_details_card_and_accountants_and_visitors_do_not()
    {
        var client = await RegisterAsync(AccountRole.Client, "Olive Reyes");
        var accountant = await RegisterAsync(AccountRole.Accountant, "Nina Ocampo");

        var page = await client.Client.GetHtmlAsync("/account");
        Assert.Contains("""<section class="details-card" id="details" aria-labelledby="details-heading">""", page);
        Assert.Contains(">Your details</h2>", page);
        Assert.Contains("""href="/account?edit=details#details">""", page);
        Assert.Equal(3, Occurrences(page, ">Not added</dd>"));
        Assert.Contains("Shared with the accountants you contact. Never shown publicly.", page);
        Assert.True(page.IndexOf("details-card", StringComparison.Ordinal) < page.IndexOf("""<ul class="menu">""", StringComparison.Ordinal));

        Assert.DoesNotContain("details-card", await accountant.Client.GetHtmlAsync("/account"));
        Assert.DoesNotContain("details-card", await app.Factory.CreateHttpsClient().GetHtmlAsync("/account"));
    }

    [DatabaseFact]
    public async Task Edit_details_shows_the_form_in_the_card_with_the_saved_name_and_the_sign_in_email()
    {
        var client = await RegisterAsync(AccountRole.Client, "Olive Reyes");

        var page = await client.Client.GetHtmlAsync(EditPage);

        Assert.Contains(">Edit your details</h2>", page);
        Assert.Contains("""name="DetailsForm.Name" type="text" value="Olive Reyes" autocomplete="name" """.TrimEnd(), page);
        foreach (var field in new[] { "Phone", "City", "BusinessName", "BusinessType", "Industry" })
        {
            Assert.Contains($"""name="DetailsForm.{field}" """.TrimEnd(), page);
        }

        foreach (var type in ClientDetailsRules.BusinessTypes)
        {
            Assert.Contains($"""<option value="{type}">{type}</option>""", page);
        }

        Assert.Contains(client.Email + "<span class=\"hint\">From your Google sign-in. Not shown to accountants.</span>", page);
        Assert.Contains("Shared with the accountants you contact, once you request a service. Never shown publicly.", page);
        Assert.Contains("Don't add your TIN or other ID numbers here.", page);
        Assert.Contains(">Save details</button>", page);
        Assert.Contains("""<a class="btn" href="/account#details">Cancel</a>""", page);
    }

    [DatabaseFact]
    public async Task Saving_stores_the_details_renews_the_session_and_shows_them_on_the_card()
    {
        var client = await RegisterAsync(AccountRole.Client, "Olive Reyes");

        var saved = await SaveAsync(client, "  Olive Santos ", "+63 900 000 0201", "Pasig City", "Santos Bakeshop", "Sole proprietor", "Food and drink");

        Assert.Equal(HttpStatusCode.Redirect, saved.StatusCode);
        Assert.StartsWith("/account?details=saved", Target(saved));
        Assert.Contains(Cookies(saved), c => c.StartsWith(".AspNetCore.Identity.Application=", StringComparison.Ordinal));
        await using (var db = app.Schema.CreateContext())
        {
            var user = await db.Users.SingleAsync(u => u.Id == client.Id);
            Assert.Equal("Olive Santos", user.DisplayName);
            Assert.Equal(new ClientDetails("+63 900 000 0201", "Pasig City", "Santos Bakeshop", "Sole proprietor", "Food and drink"), user.ClientDetails);
        }

        var page = await client.Client.GetHtmlAsync("/account?details=saved");
        Assert.Contains("<dt>Phone</dt><dd>+63 900 000 0201</dd>", page);
        Assert.Contains("<dt>City</dt><dd>Pasig City</dd>", page);
        Assert.Contains("<dt>Business</dt><dd>Santos Bakeshop · Sole proprietor · Food and drink</dd>", page);
        Assert.Contains("""<p class="save-status ok" role="status">Saved. The accountants you contact see these details.</p>""", page);
        Assert.Contains("<h2>Olive Santos</h2>", page); // The renewed session carries the new name.

        // Clearing optional fields stores them as nothing again.
        await SaveAsync(client, "Olive Santos", " ", "", "", "", "");
        await using (var db = app.Schema.CreateContext())
        {
            Assert.True((await db.Users.SingleAsync(u => u.Id == client.Id)).ClientDetails.IsEmpty);
        }
    }

    [DatabaseFact]
    public async Task Refused_details_are_not_stored_and_the_form_keeps_what_was_typed()
    {
        var client = await RegisterAsync(AccountRole.Client, "Olive Reyes");

        var refused = await SaveAsync(client, "", "call me", "Pasig City", "Santos Bakeshop", "Cooperative", "Food and drink");

        Assert.Equal(HttpStatusCode.OK, refused.StatusCode);
        var page = await refused.ReadHtmlAsync();
        Assert.Contains("""<span class="ferr" id="details-name-error">Enter your name.</span>""", page);
        Assert.Contains("""<span class="ferr" id="details-phone-error">Enter a phone number using digits, spaces, + or -.</span>""", page);
        Assert.Contains("""<span class="ferr" id="details-type-error">Choose a type from the list.</span>""", page);
        Assert.Contains("""value="call me" """.TrimEnd(), page);
        Assert.Contains("""value="Santos Bakeshop" """.TrimEnd(), page);
        Assert.Contains("""aria-invalid="true" aria-describedby="details-phone-error" """.TrimEnd(), page);
        await using var db = app.Schema.CreateContext();
        var user = await db.Users.SingleAsync(u => u.Id == client.Id);
        Assert.Equal("Olive Reyes", user.DisplayName);
        Assert.True(user.ClientDetails.IsEmpty);
    }

    [DatabaseFact]
    public async Task The_accountant_of_a_service_sees_the_details_and_other_people_do_not()
    {
        var accountant = await RegisterAsync(AccountRole.Accountant, "Nina Ocampo");
        var profile = await ListProfileAsync(accountant.Id, "Nina Ocampo");
        var client = await RegisterAsync(AccountRole.Client, "Olive Reyes");
        await SaveAsync(client, "Olive Reyes", "+63 900 000 0201", "Pasig City", "Reyes Print Shop", "Not registered yet", "Printing");
        var service = await RequestAsync(client, profile, "Tax filing");

        var request = await accountant.Client.GetHtmlAsync($"/work/{service}");
        Assert.Contains("""<section class="about-client is-compact" aria-label="About Olive Reyes">""", request);
        Assert.Contains("<h3>About Olive</h3>", request);
        Assert.Contains("<dt>Business</dt><dd>Reyes Print Shop · Not registered yet · Printing</dd>", request);
        Assert.True(request.IndexOf("about-client", StringComparison.Ordinal) < request.IndexOf(">Accept request</button>", StringComparison.Ordinal));

        var clientPage = await accountant.Client.GetHtmlAsync($"/clients/{client.Id}");
        Assert.Contains("""<section class="about-client" aria-label="About Olive Reyes">""", clientPage);
        Assert.Contains("<dt>Phone</dt><dd>+63 900 000 0201</dd>", clientPage);
        Assert.Contains("Shared by Olive with the accountants they contact.", clientPage);

        // The client sees their own details on Account, not on the service; another accountant gets nothing.
        Assert.DoesNotContain("about-client", await client.Client.GetHtmlAsync($"/work/{service}"));
        Assert.Contains("Shared with the accountants you contact: <strong>Nina Ocampo</strong>.", await client.Client.GetHtmlAsync(EditPage));
        var stranger = await RegisterAsync(AccountRole.Accountant, "Paolo Lim");
        var strangerPage = await stranger.Client.GetAsync($"/clients/{client.Id}");
        Assert.Equal(HttpStatusCode.NotFound, strangerPage.StatusCode);
        Assert.DoesNotContain("+63 900 000 0201", await strangerPage.Content.ReadAsStringAsync());
        Assert.DoesNotContain("+63 900 000 0201", await app.Factory.CreateHttpsClient().GetStringAsync($"/accountants/{profile.Value}"));
    }

    [DatabaseFact]
    public async Task Deleting_the_account_removes_the_details()
    {
        var client = await RegisterAsync(AccountRole.Client, "Olive Reyes");
        await SaveAsync(client, "Olive Reyes", "+63 900 000 0201", "Pasig City", "Santos Bakeshop", "Sole proprietor", "Food and drink");

        var confirm = await client.Client.GetHtmlAsync("/account/delete?step=confirm");
        await client.Client.PostAsync("/account/delete?step=confirm", new FormUrlEncodedContent(HiddenFields(confirm, "Are you sure?")));

        await using var db = app.Schema.CreateContext();
        var user = await db.Users.SingleAsync(u => u.Id == client.Id);
        Assert.NotNull(user.DeletedAt);
        Assert.True(user.ClientDetails.IsEmpty);
    }

    private sealed record Person(HttpClient Client, Guid Id, string Email);

    private async Task<Person> RegisterAsync(AccountRole role, string name)
    {
        var email = $"{Guid.NewGuid():N}@example.test";
        var client = app.Factory.CreateHttpsClient();
        await TestAccounts.RegisterAsync(client, role, name, email);
        await using var db = app.Schema.CreateContext();
        return new Person(client, (await db.Users.SingleAsync(u => u.Email == email)).Id, email);
    }

    private static async Task<HttpResponseMessage> SaveAsync(Person client, string name, string phone, string city, string business, string type, string industry)
    {
        var page = await client.Client.GetHtmlAsync(EditPage);
        var fields = HiddenFields(page, "Edit your details");
        fields["DetailsForm.Name"] = name;
        fields["DetailsForm.Phone"] = phone;
        fields["DetailsForm.City"] = city;
        fields["DetailsForm.BusinessName"] = business;
        fields["DetailsForm.BusinessType"] = type;
        fields["DetailsForm.Industry"] = industry;
        return await client.Client.PostAsync(EditPage, new FormUrlEncodedContent(fields));
    }

    private async Task<AccountantId> ListProfileAsync(Guid owner, string name)
    {
        var profile = AccountantProfile.CreateFor(owner, name, "nina@example.test");
        profile.Edit(name, "Certified Public Accountant", "Taguig City", "Synthetic summary.", "nina@example.test", null, null, true);
        profile.ReplaceServices([("Tax filing", "")]);
        profile.MarkSaved();
        await using var db = app.Schema.CreateContext();
        db.AccountantProfiles.Add(profile);
        await db.SaveChangesAsync();
        return profile.Id;
    }

    private static async Task<Guid> RequestAsync(Person client, AccountantId profile, string service)
    {
        var page = await client.Client.GetHtmlAsync($"/accountants/{profile.Value}/request-service");
        var fields = HiddenFields(page, "will review your request");
        fields["Form.Service"] = service;
        var target = Target(await client.Client.PostAsync($"/accountants/{profile.Value}/request-service", new FormUrlEncodedContent(fields)))!;
        return Guid.Parse(target["/work/".Length..target.IndexOf('?')]);
    }

    private static int Occurrences(string text, string value)
    {
        var count = 0;
        for (var i = text.IndexOf(value, StringComparison.Ordinal); i >= 0; i = text.IndexOf(value, i + value.Length, StringComparison.Ordinal))
        {
            count++;
        }

        return count;
    }
}
