using TaxBuddy.Web.Application.Profiles;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Tests.Application;

/// <summary>F08 PH02–PH04: portfolio highlight rules (PRD BR13; D23 provisional limits).</summary>
public sealed class HighlightTests
{
    [Fact]
    public void Complete_rows_are_kept_in_order_and_blank_rows_are_dropped()
    {
        var form = Form(
            new HighlightInput { Title = " Payroll set-up ", ClientType = "Staffing", Work = "Designed the payroll process.", Result = "Runs on its own." },
            new HighlightInput(),
            new HighlightInput { Title = "Audit", Work = "Led the audit.", ClientType = "  " });

        var (profile, errors) = ProfileValidator.Validate(form);

        Assert.Empty(errors);
        Assert.Equal(
            [("Payroll set-up", "Staffing", "Designed the payroll process.", "Runs on its own."), ("Audit", null, "Led the audit.", null)],
            profile!.Highlights);
    }

    [Fact]
    public void A_half_filled_row_names_each_missing_part_with_the_prototypes_words()
    {
        var (profile, errors) = ProfileValidator.Validate(Form(new HighlightInput(), new HighlightInput { ClientType = "Food retail" }));

        Assert.Null(profile);
        Assert.Equal("Highlight 2: add a title.", errors["Highlights[1].Title"]);
        Assert.Equal("Highlight 2: describe what you did.", errors["Highlights[1].Work"]);
        Assert.Equal(2, errors.Count);
    }

    [Fact]
    public void More_than_six_or_text_over_the_limits_is_refused()
    {
        var seven = Enumerable.Range(1, 7).Select(i => new HighlightInput { Title = $"Work {i}", Work = "Did it." }).ToArray();
        Assert.Equal("Keep to 6 highlights or fewer.", ProfileValidator.Validate(Form(seven)).Errors["Highlights"]);

        var (_, errors) = ProfileValidator.Validate(Form(new HighlightInput
        {
            Title = new string('t', ProfileLimits.HighlightTitle + 1),
            ClientType = new string('c', ProfileLimits.HighlightClientType + 1),
            Work = new string('w', ProfileLimits.HighlightWork + 1),
            Result = new string('r', ProfileLimits.HighlightResult + 1),
        }));
        Assert.Equal("Highlight 1: the title can be at most 100 characters (now 101).", errors["Highlights[0].Title"]);
        Assert.Contains("at most 60 characters", errors["Highlights[0].ClientType"]);
        Assert.Contains("at most 600 characters", errors["Highlights[0].Work"]);
        Assert.Contains("at most 300 characters", errors["Highlights[0].Result"]);
    }

    [Fact]
    public void The_profile_itself_refuses_a_seventh_highlight_or_one_without_a_title_or_work()
    {
        var profile = AccountantProfile.CreateFor(Guid.NewGuid(), "Andrea Villanueva", "andrea@example.test");
        profile.ReplaceHighlights(Enumerable.Range(1, 6).Select(i => ($"Work {i}", (string?)null, "Did it.", (string?)null)));

        Assert.Equal(6, profile.Highlights.Count);
        Assert.Equal([0, 1, 2, 3, 4, 5], profile.Highlights.Select(h => h.Position));
        Assert.Throws<InvalidOperationException>(() => profile.AddHighlight("Seventh", null, "Did it.", null));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            profile.ReplaceHighlights(Enumerable.Range(1, 7).Select(i => ($"Work {i}", (string?)null, "Did it.", (string?)null))));
        profile.ReplaceHighlights([]);
        Assert.Throws<ArgumentException>(() => profile.AddHighlight(" ", null, "Did it.", null));
        Assert.Throws<ArgumentException>(() => profile.AddHighlight("Title", null, "", null));
    }

    [Fact]
    public void Highlights_do_not_affect_listing()
    {
        var profile = AccountantProfile.CreateFor(Guid.NewGuid(), "Andrea Villanueva", "andrea@example.test");
        profile.AddHighlight("Payroll set-up", null, "Designed the payroll process.", null);

        Assert.False(profile.IsListed); // still missing title, summary, services and consent (BR01)
    }

    private static ProfileForm Form(params HighlightInput[] highlights)
    {
        var form = ProfileForm.StartingFrom("Andrea Villanueva");
        form.Highlights = [.. highlights];
        return form;
    }
}
