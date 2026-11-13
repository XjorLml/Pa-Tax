using TaxBuddy.TestSupport;
using TaxBuddy.Web.Application.Directory;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Tests.Application;

/// <summary>Address handling and directory states (SDD navigation <c>/</c>; PRD R01, R02; FT02, FT05).</summary>
public sealed class DirectoryServiceTests
{
    [Fact]
    public void Search_text_is_trimmed_collapsed_and_capped()
    {
        Assert.Equal("tax filing", DirectoryQuery.From("  tax \t  filing ", null, null).SearchText);
        Assert.Null(DirectoryQuery.From("   ", null, null).SearchText);
        Assert.Equal(DirectoryQuery.MaxSearchLength, DirectoryQuery.From(new string('a', 500), null, null).SearchText!.Length);
    }

    [Fact]
    public void Unknown_or_malformed_values_are_ignored()
    {
        var query = DirectoryQuery.From(null, ["Crypto consulting"], "abc");

        Assert.Null(query.Service);
        Assert.Equal(1, query.Page);
        Assert.False(query.IsFiltered);
    }

    [Theory]
    [InlineData("2", 2)]
    [InlineData("0", 1)]
    [InlineData("-1", 1)]
    [InlineData("1.5", 1)]
    [InlineData("99999", 1)]
    [InlineData(null, 1)]
    public void Page_must_be_a_positive_whole_number(string? page, int expected)
    {
        Assert.Equal(expected, DirectoryQuery.From(null, null, page).Page);
    }

    [Fact]
    public void Last_service_value_wins_and_an_empty_one_clears_the_filter()
    {
        // Wide screens send the menu's value first and the pressed button's value last (SDD render modes).
        Assert.Equal("Payroll", DirectoryQuery.From(null, ["Audit", "payroll"], null).Service);
        Assert.Null(DirectoryQuery.From(null, ["Audit", ""], null).Service);
    }

    [Fact]
    public async Task Empty_directory_is_distinct_from_no_match()
    {
        var empty = new DirectoryService(new FakeStore(new DirectoryRows([], 0, 1, AnyListed: false)), Clock());
        var noMatch = new DirectoryService(new FakeStore(new DirectoryRows([], 0, 1, AnyListed: true)), Clock());

        var emptyResult = await empty.SearchAsync(DirectoryQuery.Default);
        var noMatchResult = await noMatch.SearchAsync(DirectoryQuery.From("zzz", null, null));

        Assert.True(emptyResult.DirectoryIsEmpty);
        Assert.False(emptyResult.NoMatch);
        Assert.False(noMatchResult.DirectoryIsEmpty);
        Assert.True(noMatchResult.NoMatch);
    }

    [Fact]
    public async Task Cards_get_page_count_and_years_of_experience()
    {
        var row = new CardRow(AccountantId.New(), "Andrea Villanueva", "CPA", "Makati City", "Summary", ["Bookkeeping"],
            [new YearMonth(2021, 3), new YearMonth(2014, 1)]);
        var service = new DirectoryService(new FakeStore(new DirectoryRows([row], 13, 2, AnyListed: true)), Clock());

        var result = await service.SearchAsync(DirectoryQuery.From(null, null, "2"));

        Assert.Equal(3, result.PageCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(12, Assert.Single(result.Cards).YearsOfExperience);
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData(null)]
    public async Task Malformed_profile_ids_are_not_available_without_touching_the_store(string? id)
    {
        var store = new FakeStore(new DirectoryRows([], 0, 1, false));

        Assert.Null(await new DirectoryService(store, Clock()).GetProfileAsync(id));
        Assert.Equal(0, store.FindCalls);
    }

    [Fact]
    public void Website_becomes_a_link_only_when_it_is_a_web_address()
    {
        var withHost = new AccountantProfile(AccountantId.New(), "B", "T", null, "S", "b@example.com", null, "benjieramos.example.com", true);
        var script = new AccountantProfile(AccountantId.New(), "B", "T", null, "S", "b@example.com", null, "javascript:alert(1)", true);
        var noDot = new AccountantProfile(AccountantId.New(), "B", "T", null, "S", "b@example.com", null, "my website", true);

        Assert.Equal("https://benjieramos.example.com/", AccountantProfileView.From(withHost, new DateOnly(2026, 11, 13)).WebsiteHref);
        Assert.Null(AccountantProfileView.From(script, new DateOnly(2026, 11, 13)).WebsiteHref);
        Assert.Null(AccountantProfileView.From(noDot, new DateOnly(2026, 11, 13)).WebsiteHref);
    }

    private static FixedClock Clock() => new(new DateTimeOffset(2026, 11, 13, 2, 0, 0, TimeSpan.Zero));

    private sealed class FakeStore(DirectoryRows rows) : IDirectoryStore
    {
        public int FindCalls { get; private set; }

        public Task<DirectoryRows> SearchAsync(DirectoryQuery query, int pageSize, CancellationToken cancellationToken) => Task.FromResult(rows);

        public Task<AccountantProfile?> FindListedAsync(AccountantId id, CancellationToken cancellationToken)
        {
            FindCalls++;
            return Task.FromResult<AccountantProfile?>(null);
        }
    }
}
