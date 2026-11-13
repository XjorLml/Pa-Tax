using TaxBuddy.TestSupport;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Tests.Domain;

/// <summary>PRD BR01 (listing rule), D04 (fields, provisional) and BR07 (months without a zone).</summary>
public sealed class ProfileRulesTests
{
    [Fact]
    public void Complete_profile_is_listed()
    {
        var profile = Profiles.Listed("Andrea Villanueva");

        Assert.True(profile.IsListed);
        Assert.Empty(ListingRules.MissingForListing(profile));
    }

    [Theory]
    [InlineData("name")]
    [InlineData("title")]
    [InlineData("summary")]
    [InlineData("email")]
    public void Profile_missing_a_required_text_field_is_not_listed(string missing)
    {
        var profile = new AccountantProfile(
            AccountantId.New(),
            missing == "name" ? "  " : "Andrea Villanueva",
            missing == "title" ? "" : "Certified Public Accountant",
            "Makati City",
            missing == "summary" ? " " : "Clean books, filed on time.",
            missing == "email" ? "" : "andrea@example.com",
            null,
            null,
            consentToShowContact: true);
        profile.AddService("Bookkeeping", "Monthly books.");

        Assert.False(profile.IsListed);
        Assert.Single(ListingRules.MissingForListing(profile));
    }

    [Fact]
    public void Profile_without_consent_or_services_is_not_listed()
    {
        Assert.False(Profiles.WithoutConsent("Sam Aquino").IsListed);
        Assert.False(Profiles.WithoutServices("Sam Aquino").IsListed);
        Assert.Contains("agreement to show contact details", ListingRules.MissingForListing(Profiles.WithoutConsent("Sam Aquino")));
    }

    [Fact]
    public void Services_come_from_the_catalog_once_each_in_catalog_spelling()
    {
        var profile = Profiles.WithoutServices("Carla Mendoza");

        profile.AddService("  tax FILING ", "Returns.");

        Assert.Equal("Tax filing", Assert.Single(profile.Services).Name);
        Assert.Throws<ArgumentException>(() => profile.AddService("Tax filing", "Again."));
        Assert.Throws<ArgumentException>(() => profile.AddService("Crypto consulting", "Not in the catalog."));
    }

    [Fact]
    public void Text_is_trimmed_and_capped()
    {
        var profile = new AccountantProfile(AccountantId.New(), "  Daniel Cruz  ", "Auditor", "", "Audits.", "d@example.com", " ", null, true);

        Assert.Equal("Daniel Cruz", profile.DisplayName);
        Assert.Null(profile.City);
        Assert.Null(profile.Phone);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new AccountantProfile(AccountantId.New(), new string('x', ProfileLimits.DisplayName + 1), "t", null, "s", "e@example.com", null, null, true));
    }

    [Fact]
    public void Experience_cannot_end_before_it_starts()
    {
        var profile = Profiles.WithoutServices("Elaine Navarro");

        Assert.Throws<ArgumentException>(() => profile.AddExperience("Tax adviser", "Synthetic Firm", new YearMonth(2020, 5), new YearMonth(2020, 4)));
        profile.AddExperience("Tax adviser", "Synthetic Firm", new YearMonth(2020, 5), new YearMonth(2020, 5));
        Assert.Single(profile.Experience);
    }

    [Fact]
    public void Year_month_parses_compares_and_displays()
    {
        var march = YearMonth.Parse("2021-03");

        Assert.Equal(new YearMonth(2021, 3), march);
        Assert.Equal("2021-03", march.ToString());
        Assert.Equal("Mar 2021", march.ToDisplayString());
        Assert.Equal(13, march.MonthsUntil(new YearMonth(2022, 4)));
        Assert.True(march.CompareTo(new YearMonth(2021, 4)) < 0);
        Assert.Throws<FormatException>(() => YearMonth.Parse("2021-13"));
        Assert.Throws<FormatException>(() => YearMonth.Parse("March 2021"));
    }

    [Fact]
    public void Years_of_experience_count_whole_years_from_the_earliest_start()
    {
        var today = new DateOnly(2026, 11, 13);

        Assert.Null(ExperienceYears.Calculate([], today));
        Assert.Null(ExperienceYears.Calculate([new YearMonth(2026, 1)], today));
        Assert.Equal(1, ExperienceYears.Calculate([new YearMonth(2025, 11)], today));
        Assert.Equal(12, ExperienceYears.Calculate([new YearMonth(2021, 3), new YearMonth(2014, 1), new YearMonth(2016, 6)], today));
    }

    [Fact]
    public void Manila_date_is_used_for_today()
    {
        // 16:30 UTC on 31 December is already 1 January in Asia/Manila (UTC+8), PRD BR07.
        var clock = new FixedClock(new DateTimeOffset(2026, 12, 31, 16, 30, 0, TimeSpan.Zero));

        Assert.Equal(new DateOnly(2027, 1, 1), ManilaTime.Today(clock));
    }

    [Theory]
    [InlineData("5eed0000-0000-4000-8000-000000000001", true)]
    [InlineData("00000000-0000-0000-0000-000000000000", false)]
    [InlineData("not-a-guid", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void Accountant_ids_must_be_real_guids(string? text, bool valid)
    {
        Assert.Equal(valid, AccountantId.TryParse(text, out _));
    }
}
