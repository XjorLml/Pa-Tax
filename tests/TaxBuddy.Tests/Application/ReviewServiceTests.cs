using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TaxBuddy.TestSupport;
using TaxBuddy.Web.Application.Chat;
using TaxBuddy.Web.Application.Reviews;
using TaxBuddy.Web.Data;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Tests.Application;

/// <summary>F09 review rules without a database (PRD BR10, D21, D23 provisional).</summary>
public sealed class ReviewRuleTests
{
    [Theory]
    [InlineData("Liza Fernandez", "Liza F.")]
    [InlineData("  maria   de la cruz ", "maria C.")]
    [InlineData("Paolo", "Paolo")]
    [InlineData("", "A client")]
    [InlineData(null, "A client")]
    public void A_reviewer_is_named_by_first_name_and_last_initial(string? name, string expected) =>
        Assert.Equal(expected, ReviewerName.Short(name));

    [Theory]
    [InlineData(new[] { 5, 4 }, "4.5")]
    [InlineData(new[] { 5, 5, 4 }, "4.7")]
    [InlineData(new[] { 4, 4, 5, 5, 5, 5, 4, 4 }, "4.5")]
    [InlineData(new[] { 3 }, "3.0")]
    public void The_average_is_shown_to_one_decimal(int[] stars, string expected)
    {
        var rating = new RatingSummary(stars.Length, stars.Sum());

        Assert.Equal(expected, rating.Average!.Value.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture));
    }

    [Fact]
    public void No_reviews_has_no_average_rather_than_zero() => Assert.Null(RatingSummary.None.Average);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("0")]
    [InlineData("6")]
    [InlineData("4.5")]
    [InlineData("five")]
    public void Stars_must_be_a_whole_number_from_1_to_5(string? stars)
    {
        var errors = ReviewService.Validate(new ReviewForm { Stars = stars }, out _, out _);

        Assert.Equal("Choose a rating from 1 to 5 stars.", errors[ReviewService.StarsKey]);
    }

    [Fact]
    public void The_text_is_optional_trimmed_and_at_most_1000_characters()
    {
        Assert.Empty(ReviewService.Validate(new ReviewForm { Stars = "4", Text = "   " }, out var stars, out var empty));
        Assert.Equal(4, stars);
        Assert.Null(empty);

        Assert.Empty(ReviewService.Validate(new ReviewForm { Stars = "5", Text = " " + new string('a', 1000) + "\r\n" }, out _, out var full));
        Assert.Equal(1000, full!.Length);

        var errors = ReviewService.Validate(new ReviewForm { Stars = "5", Text = new string('a', 1001) }, out _, out _);
        Assert.Equal("Your review can be at most 1,000 characters (now 1,001).", errors[ReviewService.TextKey]);
    }

    [Fact]
    public void The_review_itself_refuses_bad_stars_long_text_and_an_empty_or_long_reply()
    {
        var at = new DateTimeOffset(2026, 11, 17, 0, 0, 0, TimeSpan.Zero);
        Assert.Throws<ArgumentOutOfRangeException>(() => new Review(Guid.NewGuid(), AccountantId.New(), Guid.NewGuid(), 0, null, at));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Review(Guid.NewGuid(), AccountantId.New(), Guid.NewGuid(), 6, null, at));
        Assert.Throws<ArgumentException>(() => new Review(Guid.NewGuid(), AccountantId.New(), Guid.NewGuid(), 5, new string('a', 1001), at));

        var review = new Review(Guid.NewGuid(), AccountantId.New(), Guid.NewGuid(), 5, "Great", at);
        Assert.Throws<ArgumentException>(() => review.SetReply("  ", at));
        Assert.Throws<ArgumentException>(() => review.SetReply(new string('a', 601), at));
        review.SetReply(" Thank you. ", at);
        Assert.Equal("Thank you.", review.ReplyText);
    }
}

/// <summary>
/// F09 review rules on the real PostgreSQL provider, in a throwaway schema (RV01–RV06; BR09–BR11). Each test creates its
/// own accounts, so the tests share the schema without seeing each other's reviews.
/// </summary>
public sealed class ReviewServiceTests(SchemaFixture fixture) : IClassFixture<SchemaFixture>
{
    private static readonly DateTimeOffset Now = new(2026, 11, 17, 6, 0, 0, TimeSpan.Zero);

    private ReviewService Reviews(DateTimeOffset? at = null) =>
        new(new ReviewStore(fixture.Schema, NullLogger<ReviewStore>.Instance), new FixedClock(at ?? Now));

    private ChatService Chat => new(new ChatStore(fixture.Schema, NullLogger<ChatStore>.Instance), new SilentNotifier(), new FixedClock(Now));

    [DatabaseFact]
    public async Task A_client_can_review_only_after_the_accountant_has_sent_a_message()
    {
        var (client, accountant, profile) = await PairAsync();
        var reviews = Reviews();

        Assert.Equal(ReviewAccess.NoReplyYet, (await reviews.OpenFormAsync(client, profile))!.Access); // No chat yet.
        var sent = await Chat.SendAsync(client, null, profile, Guid.NewGuid(), "Hi! Can you help?");
        Assert.Equal(ReviewAccess.NoReplyYet, (await reviews.OpenFormAsync(client, profile))!.Access); // Only the client wrote.
        var refused = await reviews.SaveAsync(client, profile, new ReviewForm { Stars = "5" });
        Assert.Equal(ReviewOutcome.NotAllowed, refused.Outcome);
        Assert.Equal(ReviewAccess.NoReplyYet, refused.Access);

        await Chat.SendAsync(accountant, sent.ConversationId, null, Guid.NewGuid(), "Yes, happy to.");

        var form = (await reviews.OpenFormAsync(client, profile))!;
        Assert.Equal(ReviewAccess.Allowed, form.Access);
        Assert.Equal("Client P.", form.ReviewerName);
        Assert.Null(form.Saved);
        Assert.Equal(ReviewOutcome.Posted, (await reviews.SaveAsync(client, profile, new ReviewForm { Stars = "5", Text = " Clear and quick. " })).Outcome);
    }

    [DatabaseFact]
    public async Task Visitors_accountants_and_unlisted_profiles_cannot_be_reviewed()
    {
        var (_, accountant, profile) = await PairAsync();
        var reviews = Reviews();

        Assert.Equal(ReviewAccess.SignIn, (await reviews.OpenFormAsync(null, profile))!.Access);
        Assert.Equal(ReviewAccess.NotAClient, (await reviews.OpenFormAsync(accountant, profile))!.Access);
        Assert.Equal(ReviewOutcome.NotAllowed, (await reviews.SaveAsync(accountant, profile, new ReviewForm { Stars = "5" })).Outcome);
        Assert.Null(await reviews.OpenFormAsync(null, AccountantId.New()));
        Assert.Equal(ReviewOutcome.Gone, (await reviews.SaveAsync(accountant, AccountantId.New(), new ReviewForm { Stars = "5" })).Outcome);
    }

    [DatabaseFact]
    public async Task There_is_one_review_per_client_and_accountant_which_the_client_changes_or_deletes()
    {
        var (client, _, profile) = await RepliedPairAsync();

        Assert.Equal(ReviewOutcome.Posted, (await Reviews().SaveAsync(client, profile, new ReviewForm { Stars = "3", Text = "Good." })).Outcome);
        var updated = await Reviews(Now.AddMinutes(5)).SaveAsync(client, profile, new ReviewForm { Stars = "5", Text = "Even better after the follow-up." });
        Assert.Equal(ReviewOutcome.Updated, updated.Outcome);

        var page = await Reviews().ForProfileAsync(client, profile);
        var only = Assert.Single(page.Reviews);
        Assert.Equal(5, only.Stars);
        Assert.Equal("Even better after the follow-up.", only.Text);
        Assert.True(only.IsMine);
        Assert.True(page.HasReview);
        Assert.Equal("5.0", page.Rating.Average!.Value.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture));
        Assert.Equal(Now.AddMinutes(5), only.At); // Listed by the last change.
        Assert.Equal("5", (await Reviews().OpenFormAsync(client, profile))!.Saved!.Stars);

        Assert.Equal(ReviewOutcome.Deleted, (await Reviews().DeleteAsync(client, profile)).Outcome);
        Assert.Empty((await Reviews().ForProfileAsync(null, profile)).Reviews);
    }

    [DatabaseFact]
    public async Task An_invalid_review_saves_nothing()
    {
        var (client, _, profile) = await RepliedPairAsync();

        var result = await Reviews().SaveAsync(client, profile, new ReviewForm { Text = "Forgot the stars." });

        Assert.Equal(ReviewOutcome.Invalid, result.Outcome);
        Assert.Contains(ReviewService.StarsKey, result.Errors!.Keys);
        Assert.Empty((await Reviews().ForProfileAsync(null, profile)).Reviews);
    }

    [DatabaseFact]
    public async Task Reviews_are_listed_newest_first_with_the_average_and_short_names_only()
    {
        var (first, accountant, profile) = await RepliedPairAsync();
        var second = await AnotherRepliedClientAsync(accountant, profile, "Rica Domingo");
        await Reviews().SaveAsync(first, profile, new ReviewForm { Stars = "4" });
        await Reviews(Now.AddMinutes(1)).SaveAsync(second, profile, new ReviewForm { Stars = "5", Text = "Set up our payroll in a week." });

        var page = await Reviews().ForProfileAsync(null, profile);

        Assert.Equal(["Rica D.", "Client P."], page.Reviews.Select(r => r.ReviewerName));
        Assert.Equal(2, page.Rating.Count);
        Assert.Equal(4.5m, page.Rating.Average);
        Assert.All(page.Reviews, r => Assert.False(r.IsMine));
        Assert.Null(page.Reviews[1].Text);
        Assert.False(page.IsOwner);
    }

    [DatabaseFact]
    public async Task Only_the_reviewed_accountant_replies_once_per_review_and_can_edit_it()
    {
        var (client, accountant, profile) = await RepliedPairAsync();
        await Reviews().SaveAsync(client, profile, new ReviewForm { Stars = "4", Text = "Good work." });
        var reviewId = (await Reviews().ForProfileAsync(null, profile)).Reviews.Single().Id;
        var (_, otherAccountant, _) = await PairAsync("Other Accountant");

        Assert.Equal(ReplyOutcome.NotAllowed, (await Reviews().ReplyAsync(client, profile, reviewId, "I'm the client")).Outcome);
        Assert.Equal(ReplyOutcome.NotAllowed, (await Reviews().ReplyAsync(otherAccountant, profile, reviewId, "Not mine")).Outcome);
        Assert.Equal(ReplyOutcome.NotAllowed, (await Reviews().ReplyAsync(null, profile, reviewId, "Visitor")).Outcome);
        Assert.Equal(ReplyOutcome.NotAllowed, (await Reviews().ReplyAsync(accountant, profile, Guid.NewGuid(), "Unknown review")).Outcome);
        var empty = await Reviews().ReplyAsync(accountant, profile, reviewId, "   ");
        Assert.Equal(ReplyOutcome.Invalid, empty.Outcome);
        Assert.Equal("Write a reply first.", empty.Error);
        Assert.Equal("Your reply can be at most 600 characters (now 601).",
            (await Reviews().ReplyAsync(accountant, profile, reviewId, new string('a', 601))).Error);

        Assert.Equal(ReplyOutcome.Posted, (await Reviews().ReplyAsync(accountant, profile, reviewId, "Thank you!")).Outcome);
        Assert.Equal(ReplyOutcome.Posted, (await Reviews().ReplyAsync(accountant, profile, reviewId, "Thank you, glad it helped.")).Outcome);

        var page = await Reviews().ForProfileAsync(accountant, profile);
        Assert.True(page.IsOwner);
        Assert.Equal(ReviewAccess.NotAClient, page.Access);
        Assert.Equal("Thank you, glad it helped.", page.Reviews.Single().ReplyText);

        // The client edits the review: the reply stays. Deleting the review removes it with its reply.
        await Reviews(Now.AddMinutes(2)).SaveAsync(client, profile, new ReviewForm { Stars = "5" });
        Assert.Equal("Thank you, glad it helped.", (await Reviews().ForProfileAsync(null, profile)).Reviews.Single().ReplyText);
    }

    [DatabaseFact]
    public async Task A_failed_save_reports_failed()
    {
        var reviews = new ReviewService(new ReviewStore(new UnreachableSchema(), NullLogger<ReviewStore>.Instance), new FixedClock(Now));
        var client = new ChatViewer(Guid.NewGuid(), AccountRole.Client);

        Assert.Equal(ReviewOutcome.Failed, (await reviews.SaveAsync(client, AccountantId.New(), new ReviewForm { Stars = "5" })).Outcome);
        Assert.Equal(ReviewOutcome.Failed, (await reviews.DeleteAsync(client, AccountantId.New())).Outcome);
        Assert.Equal(ReplyOutcome.Failed, (await reviews.ReplyAsync(new ChatViewer(Guid.NewGuid(), AccountRole.Accountant), AccountantId.New(), Guid.NewGuid(), "Hi")).Outcome);
    }

    private async Task<(ChatViewer Client, ChatViewer Accountant, AccountantId Profile)> RepliedPairAsync()
    {
        var (client, accountant, profile) = await PairAsync();
        var sent = await Chat.SendAsync(client, null, profile, Guid.NewGuid(), "Hi! Can you help?");
        await Chat.SendAsync(accountant, sent.ConversationId, null, Guid.NewGuid(), "Yes, happy to.");
        return (client, accountant, profile);
    }

    private async Task<ChatViewer> AnotherRepliedClientAsync(ChatViewer accountant, AccountantId profile, string name)
    {
        var client = new ChatViewer(await AccountAsync(name, AccountRole.Client), AccountRole.Client);
        var sent = await Chat.SendAsync(client, null, profile, Guid.NewGuid(), "Hello!");
        await Chat.SendAsync(accountant, sent.ConversationId, null, Guid.NewGuid(), "Hello back.");
        return client;
    }

    private async Task<(ChatViewer Client, ChatViewer Accountant, AccountantId Profile)> PairAsync(string accountantName = "Nina Accountant")
    {
        var clientId = await AccountAsync("Client Person", AccountRole.Client);
        var accountantId = await AccountAsync(accountantName, AccountRole.Accountant);
        var profile = AccountantProfile.CreateFor(accountantId, accountantName, "nina@example.test");
        profile.Edit(accountantName, "Certified Public Accountant", "Taguig City", "Synthetic summary.", "nina@example.test", null, null, true);
        profile.ReplaceServices([("Bookkeeping", "")]);
        profile.MarkSaved();
        await fixture.Schema.AddAsync(profile);
        return (new ChatViewer(clientId, AccountRole.Client), new ChatViewer(accountantId, AccountRole.Accountant), profile.Id);
    }

    private async Task<Guid> AccountAsync(string name, AccountRole role)
    {
        var id = Guid.NewGuid();
        await using var db = fixture.Schema.CreateContext();
        db.Users.Add(new TaxBuddyUser
        {
            Id = id,
            UserName = id.ToString("N"),
            Email = $"{id:N}@example.test",
            DisplayName = name,
            Role = role,
            AgreedToTermsAt = Now,
            AgreedTermsVersion = LegalText.CurrentVersion,
            CreatedAt = Now,
        });
        await db.SaveChangesAsync();
        return id;
    }

    private sealed class SilentNotifier : IChatNotifier
    {
        public IDisposable Subscribe(Guid accountId, Func<Guid, Task> changed) => throw new NotSupportedException();

        public void Changed(Conversation conversation)
        {
        }
    }

    /// <summary>A database where nothing listens, so every call fails at once.</summary>
    private sealed class UnreachableSchema : Microsoft.EntityFrameworkCore.IDbContextFactory<TaxBuddyDbContext>
    {
        public TaxBuddyDbContext CreateDbContext() => new(new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<TaxBuddyDbContext>()
            .UseNpgsql("Host=127.0.0.1;Port=1;Database=none;Username=none;Password=none;Timeout=3").Options);
    }
}
