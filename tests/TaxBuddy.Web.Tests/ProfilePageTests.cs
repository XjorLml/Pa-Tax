using System.Net;
using TaxBuddy.TestSupport;
using TaxBuddy.Web.Tests.Support;

namespace TaxBuddy.Web.Tests;

/// <summary>F01 profile page (FT04, FT05; TP01). Seed IDs come from Data/Seed/development-profiles.json.</summary>
public sealed class ProfilePageTests(SeededAppFixture app) : IClassFixture<SeededAppFixture>
{
    private const string Andrea = "5eed0000-0000-4000-8000-000000000001";
    private const string Benjie = "5eed0000-0000-4000-8000-000000000002";
    private const string SamWithoutConsent = "5eed0000-0000-4000-8000-000000000099";

    [DatabaseFact]
    public async Task Profile_shows_every_published_field_in_the_first_response()
    {
        var response = await app.Factory.CreateClient().GetAsync($"/accountants/{Andrea}");
        var html = await response.ReadHtmlAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("<title>Andrea Villanueva, Certified Public Accountant · Tax Buddy</title>", html);
        Assert.Contains($"""<link rel="canonical" href="http://localhost/accountants/{Andrea}" """.Trim(), html);
        Assert.Contains("Makati City", html);
        Assert.Contains("I help small businesses keep clean books", html);
        Assert.Contains("Senior tax associate", html);
        Assert.Contains("Northbay Accounting", html);
        Assert.Contains("Jun 2016 – Feb 2021", html);
        Assert.Contains("Mar 2021 – Present", html);
        Assert.Contains("Monthly books, bank reconciliation and ledgers.", html);
        Assert.Contains("""href="mailto:andrea.v@example.com" """.Trim(), html);
        Assert.Contains("""href="tel:+639000000101" """.Trim(), html);
        Assert.Contains($"""href="/chats/new/{Andrea}" """.Trim(), html);
    }

    [DatabaseFact]
    public async Task Website_is_a_safe_external_link()
    {
        var html = await app.Factory.CreateClient().GetHtmlAsync($"/accountants/{Benjie}");

        Assert.Contains("""href="https://benjieramos.example.com/" rel="nofollow ugc noopener noreferrer" """.Trim(), html);
    }

    [DatabaseTheory]
    [InlineData(SamWithoutConsent)] // exists but has not agreed to show contact details (BR01)
    [InlineData("1b9d6bcd-bbfd-4b2d-9b5d-ab8dfbbd4bed")] // unknown or deleted
    [InlineData("not-a-guid")] // malformed
    public async Task Unavailable_profiles_say_so_with_a_404_and_a_way_back(string id)
    {
        var response = await app.Factory.CreateClient().GetAsync($"/accountants/{id}");
        var html = await response.ReadHtmlAsync();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("This profile is no longer available", html);
        Assert.Contains("Back to the directory", html);
        Assert.DoesNotContain("Sam Aquino", html);
    }

    [DatabaseFact]
    public async Task Message_leads_to_a_coming_soon_page_in_this_slice()
    {
        var response = await app.Factory.CreateClient().GetAsync($"/chats/new/{Andrea}");
        var html = await response.ReadHtmlAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Messaging is coming soon", html);
        Assert.Contains($"""href="/accountants/{Andrea}" """.Trim(), html);
    }

    [DatabaseFact]
    public async Task Unknown_address_is_page_not_found()
    {
        var response = await app.Factory.CreateClient().GetAsync("/no-such-page");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("Page not found", await response.ReadHtmlAsync());
    }
}

/// <summary>FT06 / TP08: with the database unreachable, pages show an error with a retry and an error status, never an empty list.</summary>
public sealed class DatabaseFailureTests : IDisposable
{
    private readonly TaxBuddyAppFactory factory = new(TaxBuddyAppFactory.UnreachableDatabase);

    [Fact]
    public async Task Directory_shows_an_error_with_retry_not_an_empty_list()
    {
        var response = await factory.CreateClient().GetAsync("/?q=tax");
        var html = await response.ReadHtmlAsync();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Contains("We couldn't load the accountants", html);
        Assert.Contains("""href="/?q=tax">Try again</a>""", html);
        Assert.DoesNotContain("No accountants listed yet", html);
        Assert.DoesNotContain("No matches", html);
    }

    [Fact]
    public async Task Profile_shows_an_error_with_retry_not_unavailable()
    {
        var response = await factory.CreateClient().GetAsync("/accountants/5eed0000-0000-4000-8000-000000000001");
        var html = await response.ReadHtmlAsync();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Contains("We couldn't load this profile", html);
        Assert.DoesNotContain("no longer available", html);
    }

    public void Dispose() => factory.Dispose();
}
