using System.Net;
using TaxBuddy.TestSupport;
using TaxBuddy.Web.Domain;
using TaxBuddy.Web.Tests.Support;

namespace TaxBuddy.Web.Tests;

/// <summary>F13 information pages (PRD R17, IP01–IP04). They are static and need no database for a visitor.</summary>
public sealed class InformationPagesTests : IDisposable
{
    private readonly TaxBuddyAppFactory factory = new(TaxBuddyAppFactory.UnreachableDatabase);

    [Theory]
    [InlineData("/about", "About Tax Buddy", "Tax Buddy is where accountants show their work and people find one they trust.")]
    [InlineData("/help", "Help and contact", "How do I message an accountant?")]
    [InlineData("/for-accountants", "For accountants", "Show your work, collect reviews from the clients you help, and let new clients message you directly.")]
    public async Task Each_page_loads_signed_out(string path, string heading, string text)
    {
        var response = await factory.CreateHttpsClient().GetAsync(path);
        var html = await response.ReadHtmlAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains($">{heading}</h1>", html);
        Assert.Contains(text, html);
    }

    [Fact]
    public async Task About_and_Help_name_who_runs_the_site_and_how_to_reach_them()
    {
        var client = factory.CreateHttpsClient();
        var mail = $"""<a href="mailto:{LegalText.ContactEmail}">{LegalText.ContactEmail}</a>""";

        var about = await client.GetHtmlAsync("/about");
        var help = await client.GetHtmlAsync("/help");

        Assert.Contains($"{LegalText.Operator}, an individual based in {LegalText.OperatorPlace}.", about);
        Assert.Contains(mail, about);
        Assert.Contains(mail, help);
        Assert.Contains("How do I delete my account?", help);
        Assert.Contains("check a CPA's licence with the PRC", help);
        // Only what the site has: Microsoft only when it is configured (F15); service work, documents and emails since 2026-11-23.
        Assert.DoesNotContain("Microsoft", help);
        Assert.Contains("How do I track a service?", help);
        Assert.Contains("a PDF, JPEG or PNG of up to 10 MB", help);
        Assert.Contains("How do I stop the emails?", help);
    }

    [Fact]
    public async Task A_visitor_is_offered_to_create_a_profile_through_sign_in()
    {
        var html = await factory.CreateHttpsClient().GetHtmlAsync("/for-accountants");

        Assert.Contains("""<a class="btn btn-primary" href="/signin?returnUrl=%2Faccount%2Fprofile">Create your profile</a>""", html);
        Assert.Contains("Listing and messaging are free for now.", html);
    }

    [Fact]
    public async Task Every_page_links_to_the_information_pages_and_visitors_see_For_accountants_in_the_site_bar()
    {
        var html = await (await factory.CreateHttpsClient().GetAsync("/no-such-page")).ReadHtmlAsync();

        var footer = html[html.IndexOf("<footer class=\"sitefoot\"", StringComparison.Ordinal)..];
        foreach (var (href, text) in new[] { ("/about", "About"), ("/help", "Help"), ("/for-accountants", "For accountants"), ("/privacy", "Privacy"), ("/terms", "Terms") })
        {
            Assert.Matches($"""<a href="{href}"[^>]*>{text}</a>""", footer); // The layout's scoped CSS adds an attribute.
        }

        var siteBar = html[html.IndexOf("<header class=\"sitebar\"", StringComparison.Ordinal)..];
        Assert.Contains("""<a class="btn btn-ghost btn-sm siteuser-foracc" href="/for-accountants">For accountants</a>""", siteBar[..siteBar.IndexOf("</header>", StringComparison.Ordinal)]);
    }

    public void Dispose() => factory.Dispose();
}

/// <summary>F13 For accountants for signed-in people (IP03): an accountant goes to their profile; a client is told why not.</summary>
public sealed class InformationPagesSignedInTests(EmptyAppFixture app) : IClassFixture<EmptyAppFixture>
{
    [DatabaseFact]
    public async Task An_accountant_goes_to_their_profile_and_a_client_is_told_an_account_has_one_role()
    {
        var accountant = app.Factory.CreateHttpsClient();
        await TestAccounts.RegisterAsync(accountant, AccountRole.Accountant, "Nina Ocampo", $"{Guid.NewGuid():N}@example.test");
        var client = app.Factory.CreateHttpsClient();
        await TestAccounts.RegisterAsync(client, AccountRole.Client, "Olive Reyes", $"{Guid.NewGuid():N}@example.test");

        var forAccountant = await accountant.GetHtmlAsync("/for-accountants");
        var forClient = await client.GetHtmlAsync("/for-accountants");

        Assert.Contains("""<a class="btn btn-primary" href="/account/profile">Go to my profile</a>""", forAccountant);
        Assert.Contains("You're signed in as a client. An account is either a client or an accountant", forClient);
        Assert.DoesNotContain("Create your profile", forClient);
        Assert.DoesNotContain("siteuser-foracc", forClient); // The site bar shows the account instead.
    }
}
