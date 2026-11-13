using Microsoft.Extensions.Logging.Abstractions;
using TaxBuddy.TestSupport;
using TaxBuddy.Web.Application.Directory;
using TaxBuddy.Web.Data;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Tests.Data;

/// <summary>
/// Directory queries on the real PostgreSQL provider, in a throwaway schema (PRD BR01, BR02, D05; F01 FT03).
/// Each test clears the schema first, so tests in this class run one at a time against the same schema.
/// </summary>
public sealed class DirectoryStoreTests(SchemaFixture fixture) : IClassFixture<SchemaFixture>
{
    private const int PageSize = 6;

    private DirectoryStore Store => new(fixture.Schema, NullLogger<DirectoryStore>.Instance);

    [DatabaseFact]
    public async Task Only_listed_profiles_are_returned()
    {
        await fixture.Schema.ClearAsync();
        var listed = Profiles.Listed("Andrea Villanueva");
        var noConsent = Profiles.WithoutConsent("Sam Aquino");
        var noServices = Profiles.WithoutServices("Tess Ocampo");
        await fixture.Schema.AddAsync(listed, noConsent, noServices);

        var rows = await Store.SearchAsync(DirectoryQuery.Default, PageSize, default);

        Assert.Equal(listed.Id, Assert.Single(rows.Rows).Id);
        Assert.Equal(1, rows.MatchCount);
        Assert.NotNull(await Store.FindListedAsync(listed.Id, default));
        Assert.Null(await Store.FindListedAsync(noConsent.Id, default));
        Assert.Null(await Store.FindListedAsync(noServices.Id, default));
        Assert.Null(await Store.FindListedAsync(AccountantId.New(), default));
    }

    [DatabaseFact]
    public async Task Paging_with_equal_names_returns_everyone_exactly_once_in_a_stable_order()
    {
        await fixture.Schema.ClearAsync();
        var same = Enumerable.Range(0, 14).Select(_ => Profiles.Listed("Same Name")).ToList();
        await fixture.Schema.AddAsync([.. same, Profiles.Listed("aaron Abad"), Profiles.Listed("Zeny Zamora")]);

        var seen = new List<AccountantId>();
        var names = new List<string>();
        for (var page = 1; page <= 3; page++)
        {
            var rows = await Store.SearchAsync(DirectoryQuery.From(null, null, page.ToString()), PageSize, default);
            Assert.Equal(16, rows.MatchCount);
            Assert.Equal(page, rows.Page);
            seen.AddRange(rows.Rows.Select(r => r.Id));
            names.AddRange(rows.Rows.Select(r => r.Name));
        }

        Assert.Equal(16, seen.Count);
        Assert.Equal(16, seen.Distinct().Count());
        Assert.Equal("aaron Abad", names[0]); // A–Z ignores letter case
        Assert.Equal("Zeny Zamora", names[^1]);
        var tieOrder = seen.Skip(1).Take(14).Select(id => id.Value).ToList();
        Assert.Equal(tieOrder.OrderBy(g => g.ToString(), StringComparer.Ordinal).ToList(), tieOrder); // ID tie-breaker; uuid order is hex text order
    }

    [DatabaseFact]
    public async Task Search_matches_name_title_city_and_service_ignoring_case()
    {
        await fixture.Schema.ClearAsync();
        var andrea = Profiles.Listed("Andrea Villanueva", city: "Makati City", services: ["Bookkeeping"]);
        var benjie = Profiles.Listed("Benjie Ramos", title: "Payroll consultant", city: "Cebu City", services: ["Payroll"]);
        var grace = Profiles.Listed("Grace Tan", city: "Taguig City", services: ["Audit", "Tax advisory"]);
        await fixture.Schema.AddAsync(andrea, benjie, grace);

        async Task<IEnumerable<string>> Search(string text) =>
            (await Store.SearchAsync(DirectoryQuery.From(text, null, null), PageSize, default)).Rows.Select(r => r.Name);

        Assert.Equal(["Andrea Villanueva"], await Search("villanueva"));
        Assert.Equal(["Benjie Ramos"], await Search("PAYROLL CONSULTANT"));
        Assert.Equal(["Grace Tan"], await Search("taguig"));
        Assert.Equal(["Grace Tan"], await Search("tax adv"));
        Assert.Equal(["Andrea Villanueva", "Benjie Ramos", "Grace Tan"], await Search("city"));
    }

    [DatabaseFact]
    public async Task Search_text_is_literal_not_a_pattern()
    {
        await fixture.Schema.ClearAsync();
        await fixture.Schema.AddAsync(
            Profiles.Listed("Percent 100% Books"),
            Profiles.Listed("Under_score Ledger"),
            Profiles.Listed("Plain Name"));

        var percent = await Store.SearchAsync(DirectoryQuery.From("%", null, null), PageSize, default);
        var underscore = await Store.SearchAsync(DirectoryQuery.From("r_s", null, null), PageSize, default);

        Assert.Equal("Percent 100% Books", Assert.Single(percent.Rows).Name);
        Assert.Equal("Under_score Ledger", Assert.Single(underscore.Rows).Name);
    }

    [DatabaseFact]
    public async Task Service_filter_combines_with_search_and_no_match_knows_the_directory_is_not_empty()
    {
        await fixture.Schema.ClearAsync();
        await fixture.Schema.AddAsync(
            Profiles.Listed("Benjie Ramos", city: "Cebu City", services: ["Payroll", "Tax filing"]),
            Profiles.Listed("Carla Mendoza", city: "Quezon City", services: ["Bookkeeping", "Payroll"]),
            Profiles.Listed("Daniel Cruz", city: "Pasig City", services: ["Audit"]));

        var payroll = await Store.SearchAsync(DirectoryQuery.From(null, ["Payroll"], null), PageSize, default);
        var payrollInCebu = await Store.SearchAsync(DirectoryQuery.From("cebu", ["Payroll"], null), PageSize, default);
        var none = await Store.SearchAsync(DirectoryQuery.From("zzzz", null, null), PageSize, default);

        Assert.Equal(["Benjie Ramos", "Carla Mendoza"], payroll.Rows.Select(r => r.Name));
        Assert.Equal("Benjie Ramos", Assert.Single(payrollInCebu.Rows).Name);
        Assert.Equal(0, none.MatchCount);
        Assert.True(none.AnyListed);
    }

    [DatabaseFact]
    public async Task Empty_directory_reports_nothing_listed_and_out_of_range_pages_fall_back()
    {
        await fixture.Schema.ClearAsync();
        await fixture.Schema.AddAsync(Profiles.WithoutConsent("Sam Aquino"));

        var empty = await Store.SearchAsync(DirectoryQuery.From("sam", null, null), PageSize, default);
        Assert.False(empty.AnyListed);

        await fixture.Schema.AddAsync(Profiles.Listed("Andrea Villanueva"));
        var pastTheEnd = await Store.SearchAsync(DirectoryQuery.From(null, null, "9"), PageSize, default);
        Assert.Equal(1, pastTheEnd.Page);
        Assert.Single(pastTheEnd.Rows);
    }

    [DatabaseFact]
    public async Task Listed_profile_round_trips_services_in_order_and_experience_months()
    {
        await fixture.Schema.ClearAsync();
        var profile = new AccountantProfile(AccountantId.New(), "Andrea Villanueva", "CPA", "Makati City", "Summary",
            "andrea@example.com", "+63 900 000 0101", "andrea.example.com", true);
        profile.AddService("Tax filing", "Returns.");
        profile.AddService("Bookkeeping", "Books.");
        profile.AddExperience("Independent accountant", "Self-employed", new YearMonth(2021, 3), null);
        profile.AddExperience("Junior auditor", "Harbor Lane Audit", new YearMonth(2014, 1), new YearMonth(2016, 5));
        await fixture.Schema.AddAsync(profile);

        var loaded = await Store.FindListedAsync(profile.Id, default);
        var card = Assert.Single((await Store.SearchAsync(DirectoryQuery.Default, PageSize, default)).Rows);

        Assert.NotNull(loaded);
        Assert.Equal(["Tax filing", "Bookkeeping"], loaded.Services.OrderBy(s => s.Position).Select(s => s.Name));
        Assert.Equal(["Tax filing", "Bookkeeping"], card.Services);
        Assert.Contains(loaded.Experience, e => e.Start == new YearMonth(2014, 1) && e.End == new YearMonth(2016, 5));
        Assert.Contains(loaded.Experience, e => e.Start == new YearMonth(2021, 3) && e.IsCurrent);
        Assert.Equal("+63 900 000 0101", loaded.Phone);
    }

    [DatabaseFact]
    public async Task Development_seed_lists_eight_and_keeps_the_profile_without_consent_unlisted()
    {
        await fixture.Schema.ClearAsync();

        var (listed, unlisted) = await DevelopmentSeed.ResetAndSeedAsync(fixture.Schema);
        var rows = await Store.SearchAsync(DirectoryQuery.Default, 50, default);

        Assert.Equal((8, 1), (listed, unlisted));
        Assert.Equal(8, rows.MatchCount);
        Assert.DoesNotContain(rows.Rows, r => r.Name == "Sam Aquino");
    }
}
