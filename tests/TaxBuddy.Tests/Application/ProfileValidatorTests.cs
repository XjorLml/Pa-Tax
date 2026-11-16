using TaxBuddy.Web.Application.Profiles;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Tests.Application;

/// <summary>F05 PE03: the profile editor's input rules (PRD D04 limits, adopted 2026-11-16).</summary>
public sealed class ProfileValidatorTests
{
    [Fact]
    public void A_complete_form_is_accepted_and_normalised()
    {
        var form = Complete();
        form.Website = "andrea-cpa.example.com";
        form.City = "  ";
        form.Experience.Add(new ExperienceInput()); // an added row left empty is dropped

        var (profile, errors) = ProfileValidator.Validate(form);

        Assert.Empty(errors);
        Assert.NotNull(profile);
        Assert.Equal("https://andrea-cpa.example.com", profile.Website);
        Assert.Null(profile.City);
        Assert.Equal(new[] { ("Bookkeeping", "Monthly books."), ("Tax filing", "") }, profile.Services);
        var entry = Assert.Single(profile.Experience);
        Assert.Equal(("Senior Accountant", "Synthetic & Co.", new YearMonth(2018, 3), (YearMonth?)null), entry);
    }

    [Fact]
    public void An_unfinished_profile_is_accepted_so_it_can_be_saved_unlisted()
    {
        var form = ProfileForm.StartingFrom("Andrea Villanueva");

        var (profile, errors) = ProfileValidator.Validate(form);

        Assert.Empty(errors);
        Assert.NotNull(profile);
        Assert.Empty(profile.Services);
        Assert.Equal("", profile.Email); // the prototype starts the contact email empty
        Assert.False(profile.ConsentToShowContact);
    }

    [Theory]
    [InlineData("DisplayName", 101, "Display name can be at most 100 characters (now 101).")]
    [InlineData("Title", 101, "Professional title can be at most 100 characters")]
    [InlineData("City", 81, "City can be at most 80 characters")]
    [InlineData("Summary", 1001, "Keep the career summary under 1,000 characters.")]
    [InlineData("Email", "not-an-email", "Enter an email address like")]
    [InlineData("Email", "two words@example.com", "Enter an email address like")]
    [InlineData("Email", "name@localhost", "Enter an email address like")]
    [InlineData("Phone", "+63 (2) 8123-4567 ext 9", "Use only digits")]
    [InlineData("Phone", 31, "Phone can be at most 30 characters")]
    [InlineData("Website", "javascript:alert(1)", "Enter a web address like example.com.")]
    [InlineData("Website", "ftp://files.example.com", "Enter a web address")]
    [InlineData("Website", "not a site", "Enter a web address")]
    public void Each_invalid_field_is_reported_against_that_field(string field, object value, string message)
    {
        var form = Complete();
        var text = value as string ?? new string('x', (int)value);
        typeof(ProfileForm).GetProperty(field)!.SetValue(form, text);

        var (profile, errors) = ProfileValidator.Validate(form);

        Assert.Null(profile);
        Assert.StartsWith(message, Assert.Contains(field, errors));
    }

    [Fact]
    public void A_too_long_service_description_is_reported_against_that_service()
    {
        var form = Complete();
        form.Services[0].Description = new string('x', 201);

        var (_, errors) = ProfileValidator.Validate(form);

        Assert.StartsWith("Service 1: the description can be at most 200 characters", errors["Services[0].Description"]);
    }

    [Fact]
    public void A_half_filled_experience_row_names_each_missing_part()
    {
        var form = Complete();
        form.Experience = [new ExperienceInput { Role = "Auditor" }];

        var (_, errors) = ProfileValidator.Validate(form);

        Assert.Equal("Experience 1: add a company, or \"Self-employed\".", errors["Experience[0].Organization"]);
        Assert.Equal("Experience 1: add a start month.", errors["Experience[0].Start"]);
        Assert.Equal("Experience 1: add an end month, or tick \"I work here now\".", errors["Experience[0].End"]);
        Assert.False(errors.ContainsKey("Experience[0].Role"));
    }

    [Theory]
    [InlineData("2021-13", null, "Experience[0].Start", "Experience 1: write the start month as YYYY-MM")]
    [InlineData("March 2021", null, "Experience[0].Start", "Experience 1: write the start month as YYYY-MM")]
    [InlineData("2021-03", "2020-12", "Experience[0].End", "Experience 1: the end month must be after the start month.")]
    [InlineData("1899-12", null, "Experience[0].Start", "Experience 1: write the start month as YYYY-MM")]
    public void Months_must_be_real_and_in_order(string start, string? end, string key, string message)
    {
        var form = Complete();
        form.Experience = [new ExperienceInput { Role = "Auditor", Organization = "Synthetic Audit", Start = start, End = end, Current = end is null }];

        var (_, errors) = ProfileValidator.Validate(form);

        Assert.StartsWith(message, errors[key]);
    }

    [Fact]
    public void A_current_role_ignores_the_end_month_and_the_same_month_is_allowed()
    {
        var form = Complete();
        form.Experience =
        [
            new ExperienceInput { Role = "Auditor", Organization = "A", Start = "2020-01", End = "2019-01", Current = true },
            new ExperienceInput { Role = "Clerk", Organization = "B", Start = "2015-06", End = "2015-06" },
        ];

        var (profile, errors) = ProfileValidator.Validate(form);

        Assert.Empty(errors);
        Assert.Null(profile!.Experience[0].End);
        Assert.Equal(new YearMonth(2015, 6), profile.Experience[1].End);
    }

    [Fact]
    public void More_than_ten_experience_entries_are_refused()
    {
        var form = Complete();
        form.Experience = Enumerable.Range(0, 11)
            .Select(i => new ExperienceInput { Role = $"Role {i}", Organization = "Org", Start = "2010-01", End = "2010-02" })
            .ToList();

        var (profile, errors) = ProfileValidator.Validate(form);

        Assert.Null(profile);
        Assert.Equal("Keep to 10 experience entries or fewer.", errors["Experience"]);
    }

    [Theory]
    [InlineData("", "Service 2: choose a service, or remove the row.")]
    [InlineData("Crypto trading", "Service 2: choose a service from the list.")]
    [InlineData("Bookkeeping", "Service 2: Bookkeeping is already listed. Choose another, or remove the row.")]
    public void Each_service_row_needs_one_catalog_service_once(string name, string message)
    {
        var form = Complete();
        form.Services[1].Name = name;

        var (profile, errors) = ProfileValidator.Validate(form);

        Assert.Null(profile);
        Assert.Equal(message, errors["Services[1].Name"]);
    }

    [Fact]
    public void The_listing_checklist_follows_the_values_as_typed()
    {
        var form = ProfileForm.StartingFrom("Andrea Villanueva");

        var steps = form.ListingSteps();

        Assert.Equal(new[] { "Display name" }, steps.Where(s => s.Done).Select(s => s.Label));
        Assert.Equal("add-svc", steps.Single(s => s.Name == "at least one service").TargetId);
        Assert.All(Complete().ListingSteps(), s => Assert.True(s.Done, s.Label));
    }

    private static ProfileForm Complete()
    {
        var form = ProfileForm.StartingFrom("Andrea Villanueva");
        form.Email = "andrea@example.test";
        form.Title = "Certified Public Accountant";
        form.City = "Makati City";
        form.Summary = "Clean books, filed on time.";
        form.Phone = "+63 (2) 8123-4567";
        form.ConsentToShowContact = true;
        form.Services =
        [
            new ServiceInput { Name = "Bookkeeping", Description = "Monthly books." },
            new ServiceInput { Name = "Tax filing" },
        ];
        form.Experience = [new ExperienceInput { Role = "Senior Accountant", Organization = "Synthetic & Co.", Start = "2018-03", Current = true }];
        return form;
    }
}
