using System.Net;
using System.Text.RegularExpressions;
using TaxBuddy.TestSupport;
using TaxBuddy.Web.Tests.Support;

namespace TaxBuddy.Web.Tests;

/// <summary>F01 directory page through the whole app (FT01–FT03, FT07; TP01). Seed data: 8 listed, 1 without consent.</summary>
public sealed class DirectoryPageTests(SeededAppFixture app) : IClassFixture<SeededAppFixture>
{
    private static readonly string[] SeededNamesAToZ =
    [
        "Andrea Villanueva", "Benjie Ramos", "Carla Mendoza", "Daniel Cruz",
        "Elaine Navarro", "Francis Lim", "Grace Tan", "Hannah de Guzman",
    ];

    [DatabaseFact]
    public async Task First_response_holds_the_listed_accountants_in_order_with_title_and_canonical_address()
    {
        var response = await app.Factory.CreateClient().GetAsync("/");
        var html = await response.ReadHtmlAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("<title>Find an accountant · Tax Buddy</title>", html);
        Assert.Matches("""<link rel="canonical" href="http://localhost/"\s*/?>""", html);
        Assert.Contains("""<meta name="description" content="Browse accountants""", html);
        AssertInOrder(html, SeededNamesAToZ.Take(6));
        Assert.DoesNotContain("Grace Tan", html); // page 2 (6 per page)
        Assert.DoesNotContain("Sam Aquino", html); // no consent: never listed (BR01)
        Assert.Contains("8 accountants", html);
        Assert.DoesNotContain("Sign in", html, StringComparison.OrdinalIgnoreCase); // FT01: no sign-in prompt
    }

    [DatabaseFact]
    public async Task Page_two_has_the_rest_and_every_accountant_appears_once()
    {
        var client = app.Factory.CreateClient();
        var first = await client.GetHtmlAsync("/");
        var second = await client.GetHtmlAsync("/?page=2");

        Assert.Contains("""href="/?page=2" """.Trim(), first);
        Assert.Contains("Page 1 of 2", first);
        Assert.Contains("Page 2 of 2", second);
        Assert.Contains("""rel="prev" href="/" """.Trim(), second);
        AssertInOrder(second, SeededNamesAToZ.Skip(6));
        foreach (var name in SeededNamesAToZ)
        {
            Assert.Equal(1, Count(first, name) + Count(second, name));
        }
    }

    [DatabaseFact]
    public async Task No_match_is_distinct_and_offers_a_way_back()
    {
        var html = await app.Factory.CreateClient().GetHtmlAsync("/?q=zzzz-no-match");

        Assert.Contains("No matches", html);
        Assert.Contains("“zzzz-no-match”", html);
        Assert.Contains("Clear search and filter", html);
        Assert.DoesNotContain("No accountants listed yet", html);
        Assert.Contains("""value="zzzz-no-match" """.Trim(), html); // the search box shows what was searched
    }

    [DatabaseFact]
    public async Task Service_filter_and_search_combine_and_stay_in_page_links()
    {
        var client = app.Factory.CreateClient();
        var payroll = await client.GetHtmlAsync("/?service=Payroll");
        var payrollCebu = await client.GetHtmlAsync("/?q=cebu&service=Payroll");

        Assert.Contains("3 accountants for Payroll", payroll);
        AssertInOrder(payroll, ["Benjie Ramos", "Carla Mendoza", "Hannah de Guzman"]);
        Assert.DoesNotContain("Andrea Villanueva", payroll);
        Assert.Contains("1 accountant for Payroll matching “cebu”", payrollCebu);
        Assert.Contains("""<link rel="canonical" href="http://localhost/?q=cebu&service=Payroll" """.Trim(), payrollCebu);
        Assert.Matches("""<button type="submit" name="service" value="Payroll" class="chip" aria-pressed="true">""", payroll);
    }

    [DatabaseFact]
    public async Task Search_form_is_a_plain_get_form_that_works_without_script()
    {
        var html = await app.Factory.CreateClient().GetHtmlAsync("/");

        Assert.Matches("""<form method="get" action="/" role="search"[^>]*>""", html);
        Assert.Matches("""<input id="q" name="q" type="search"[^>]*>""", html);
        Assert.Matches("""<select id="service" name="service"[^>]*>""", html);
        // Source order: menu, then Search, then service buttons (SDD render modes; Styles/app.css .dir-form).
        var menu = html.IndexOf("""name="service" class="min-h-11""", StringComparison.Ordinal);
        var search = html.IndexOf("""class="btn btn-primary dir-submit">Search</button>""", StringComparison.Ordinal);
        var firstChip = html.IndexOf("""<button type="submit" name="service" value class="chip" """.Trim(), StringComparison.Ordinal);
        Assert.True(menu > 0 && search > menu && firstChip > search, $"menu {menu}, search {search}, chips {firstChip}");
    }

    [DatabaseFact]
    public async Task Two_service_values_redirect_to_the_one_value_address()
    {
        var response = await app.Factory.CreateClientWithoutRedirects().GetAsync("/?q=&service=Audit&service=Payroll");

        Assert.True(response.StatusCode is HttpStatusCode.Redirect or HttpStatusCode.Found or HttpStatusCode.SeeOther,
            $"Expected a redirect, got {(int)response.StatusCode}");
        var location = response.Headers.Location?.OriginalString ?? "";
        Assert.True(location == "/?service=Payroll" || location.EndsWith("localhost/?service=Payroll", StringComparison.Ordinal), location);
    }

    [DatabaseFact]
    public async Task Unknown_and_malformed_values_are_ignored()
    {
        var response = await app.Factory.CreateClient().GetAsync("/?service=Crypto&page=abc&sort=rating&extra=1");
        var html = await response.ReadHtmlAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("8 accountants", html);
        Assert.Matches("""<link rel="canonical" href="http://localhost/"\s*/?>""", html);
    }

    [DatabaseFact]
    public async Task Public_pages_ask_not_to_be_indexed_until_D19()
    {
        var response = await app.Factory.CreateClient().GetAsync("/");
        var html = await response.ReadHtmlAsync();

        Assert.Equal("noindex, nofollow", string.Join(",", response.Headers.GetValues("X-Robots-Tag")));
        Assert.Contains("""<meta name="robots" content="noindex, nofollow" """.Trim(), html);
    }

    private static int Count(string html, string text) => Regex.Matches(html, Regex.Escape(text)).Count;

    private static void AssertInOrder(string html, IEnumerable<string> names)
    {
        var last = -1;
        foreach (var name in names)
        {
            var at = html.IndexOf(name, StringComparison.Ordinal);
            Assert.True(at > last, $"'{name}' is missing or out of order");
            last = at;
        }
    }
}

/// <summary>FT02: an empty directory says so, distinctly from "no match".</summary>
public sealed class EmptyDirectoryTests(EmptyAppFixture app) : IClassFixture<EmptyAppFixture>
{
    [DatabaseFact]
    public async Task Empty_directory_has_its_own_message()
    {
        var html = await app.Factory.CreateClient().GetHtmlAsync("/");
        var searched = await app.Factory.CreateClient().GetHtmlAsync("/?q=andrea");

        Assert.Contains("No accountants listed yet", html);
        Assert.DoesNotContain("No matches", html);
        Assert.Contains("No accountants listed yet", searched);
    }
}
