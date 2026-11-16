using TaxBuddy.Web.Domain;

namespace TaxBuddy.Tests.Domain;

/// <summary>F05: the profile's own editing rules (PRD R05, BR01, D04 limits).</summary>
public sealed class ProfileEditingRulesTests
{
    private static readonly Guid Owner = Guid.Parse("0193a6c0-0000-7000-8000-00000000f005");

    [Fact]
    public void A_new_profile_belongs_to_its_account_and_starts_unlisted()
    {
        var profile = AccountantProfile.CreateFor(Owner, "Andrea Villanueva", "andrea@example.test");

        Assert.Equal(Owner, profile.OwnerAccountId);
        Assert.False(profile.IsListed);
        Assert.Equal(new[] { "professional title", "career summary", "at least one service", "agreement to show contact details" },
            ListingRules.MissingForListing(profile));
        Assert.Equal(0, profile.Version);
    }

    [Fact]
    public void A_profile_needs_an_owner()
    {
        Assert.Throws<ArgumentException>(() => AccountantProfile.CreateFor(Guid.Empty, "A", "a@example.test"));
    }

    [Fact]
    public void Provider_name_and_email_that_are_too_long_are_cut_to_fit()
    {
        var profile = AccountantProfile.CreateFor(Owner, new string('n', 150), "x@example.test");

        Assert.Equal(ProfileLimits.DisplayName, profile.DisplayName.Length);
    }

    [Fact]
    public void Editing_completes_the_profile_and_lists_it()
    {
        var profile = AccountantProfile.CreateFor(Owner, "Andrea Villanueva", "andrea@example.test");

        profile.Edit("Andrea Villanueva", "Certified Public Accountant", "Makati City", "Clean books.", "andrea@example.test",
            null, "https://example.com", consentToShowContact: true);
        profile.ReplaceServices([("Bookkeeping", "Monthly books.")]);
        profile.MarkSaved();

        Assert.True(profile.IsListed);
        Assert.Equal(1, profile.Version);
    }

    [Fact]
    public void Replacing_services_and_experience_keeps_only_the_new_entries()
    {
        var profile = AccountantProfile.CreateFor(Owner, "A", "a@example.test");
        profile.ReplaceServices([("Bookkeeping", ""), ("Audit", "")]);
        profile.ReplaceExperience([("Clerk", "A", new YearMonth(2010, 1), new YearMonth(2012, 1))]);

        profile.ReplaceServices([("Audit", "Yearly.")]);
        profile.ReplaceExperience([("Auditor", "B", new YearMonth(2012, 2), null)]);

        Assert.Equal(new[] { "Audit" }, profile.Services.Select(s => s.Name));
        Assert.Equal("Auditor", Assert.Single(profile.Experience).Role);
    }

    [Fact]
    public void A_profile_holds_at_most_ten_experience_entries()
    {
        var profile = AccountantProfile.CreateFor(Owner, "A", "a@example.test");
        var eleven = Enumerable.Range(0, 11).Select(i => ($"Role {i}", "Org", new YearMonth(2000 + i, 1), (YearMonth?)null)).ToList();

        Assert.Throws<ArgumentOutOfRangeException>(() => profile.ReplaceExperience(eleven));
        profile.ReplaceExperience(eleven.Take(10));
        Assert.Throws<InvalidOperationException>(() => profile.AddExperience("Extra", "Org", new YearMonth(2020, 1), null));
    }

    [Fact]
    public void Editing_never_accepts_text_over_the_limit()
    {
        var profile = AccountantProfile.CreateFor(Owner, "A", "a@example.test");

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            profile.Edit("A", new string('t', ProfileLimits.Title + 1), null, "", "a@example.test", null, null, false));
    }
}
