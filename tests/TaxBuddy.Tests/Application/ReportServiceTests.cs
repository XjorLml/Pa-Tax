using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TaxBuddy.TestSupport;
using TaxBuddy.Web.Application.Chat;
using TaxBuddy.Web.Application.Reports;
using TaxBuddy.Web.Application.Reviews;
using TaxBuddy.Web.Data;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Tests.Application;

/// <summary>F10 report and block rules without a database (PRD R15, BR12).</summary>
public sealed class ReportRuleTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Rude")]
    [InlineData("2")]
    [InlineData("abusiveorharassing")]
    public void A_reason_from_the_list_is_required(string? reason) =>
        Assert.Equal("Choose a reason.", ReportService.Validate(new ReportForm { Reason = reason }, out _, out _)[ReportService.ReasonKey]);

    [Fact]
    public void Details_are_optional_trimmed_and_at_most_600_characters()
    {
        Assert.Empty(ReportService.Validate(new ReportForm { Reason = "SpamOrScam", Details = "  " }, out var reason, out var none));
        Assert.Equal(ReportReason.SpamOrScam, reason);
        Assert.Null(none);
        Assert.Equal("What you add can be at most 600 characters (now 601).",
            ReportService.Validate(new ReportForm { Reason = "SpamOrScam", Details = new string('a', 601) }, out _, out _)[ReportService.DetailsKey]);
    }

    [Fact]
    public void The_reasons_are_the_prototypes_five_in_order() =>
        Assert.Equal(
            ["Fake or misleading", "Abusive or harassing", "Spam or a scam", "Shares private information", "Something else"],
            ReportLimits.Reasons.Select(ReportLimits.Text));

    [Fact]
    public void Each_side_blocks_and_unblocks_only_their_own_block()
    {
        var (client, accountant) = (Guid.NewGuid(), Guid.NewGuid());
        var conversation = new Conversation(Guid.NewGuid(), client, accountant, DateTimeOffset.UtcNow);

        conversation.Block(accountant, DateTimeOffset.UtcNow);
        conversation.Block(client, DateTimeOffset.UtcNow);
        conversation.Unblock(client);

        Assert.True(conversation.IsBlocked); // The accountant's block stays.
        Assert.True(conversation.IsBlockedBy(accountant));
        Assert.False(conversation.IsBlockedBy(client));
        Assert.Throws<InvalidOperationException>(() => conversation.Block(Guid.NewGuid(), DateTimeOffset.UtcNow));
        conversation.Unblock(accountant);
        Assert.False(conversation.IsBlocked);
    }
}

/// <summary>
/// F10 report and block rules on the real PostgreSQL provider, in a throwaway schema (RB01–RB08). Each test creates its own
/// accounts, so the tests share the schema without seeing each other's data.
/// </summary>
public sealed class ReportServiceTests(SchemaFixture fixture) : IClassFixture<SchemaFixture>
{
    private static readonly DateTimeOffset Now = new(2026, 11, 18, 6, 0, 0, TimeSpan.Zero);

    private ReportService Reports(DateTimeOffset? at = null) =>
        new(new ReportStore(fixture.Schema, NullLogger<ReportStore>.Instance), new FixedClock(at ?? Now), NullLogger<ReportService>.Instance);

    private ChatService Chat => new(new ChatStore(fixture.Schema, NullLogger<ChatStore>.Instance), new SilentNotifier(), new FixedClock(Now));

    private ReviewService Reviews => new(new ReviewStore(fixture.Schema, NullLogger<ReviewStore>.Instance), new FixedClock(Now));

    private static ReportForm Abusive => new() { Reason = nameof(ReportReason.AbusiveOrHarassing), Details = "  Rude messages.  " };

    [DatabaseFact]
    public async Task A_signed_in_user_reports_a_profile_once_and_it_is_stored_with_its_reason()
    {
        var (client, _, profile, _) = await PairAsync();

        var page = await Reports().OpenAsync(client, ReportKind.Profile, profile.Value);
        Assert.Equal(ReportAccess.Allowed, page!.Access);
        Assert.Equal("Nina Accountant's profile", page.Target!.Description);

        var sent = await Reports().SendAsync(client, ReportKind.Profile, profile.Value, Abusive);
        Assert.Equal(ReportOutcome.Sent, sent.Outcome);
        Assert.Equal($"/accountants/{profile}?reported=profile", sent.ReturnPath);

        await using var db = fixture.Schema.CreateContext();
        var stored = await db.Reports.SingleAsync(r => r.ReporterAccountId == client.AccountId);
        Assert.Equal((ReportKind.Profile, profile.Value, ReportReason.AbusiveOrHarassing, "Rude messages."), (stored.Kind, stored.TargetId, stored.Reason, stored.Details));
        Assert.Equal(profile, stored.ProfileId);

        Assert.Equal(ReportAccess.AlreadyReported, (await Reports().OpenAsync(client, ReportKind.Profile, profile.Value))!.Access);
        Assert.Equal(ReportOutcome.AlreadyReported, (await Reports().SendAsync(client, ReportKind.Profile, profile.Value, Abusive)).Outcome);
    }

    [DatabaseFact]
    public async Task Nobody_reports_their_own_profile_or_review_and_visitors_are_asked_to_sign_in()
    {
        var (client, accountant, profile, conversation) = await PairAsync(replied: true);
        await Reviews.SaveAsync(client, profile, new ReviewForm { Stars = "5" });
        var reviewId = (await Reviews.ForProfileAsync(null, profile)).Reviews.Single().Id;

        Assert.Equal(ReportAccess.OwnContent, (await Reports().OpenAsync(accountant, ReportKind.Profile, profile.Value))!.Access);
        Assert.Equal(ReportOutcome.NotAllowed, (await Reports().SendAsync(accountant, ReportKind.Profile, profile.Value, Abusive)).Outcome);
        Assert.Equal(ReportAccess.OwnContent, (await Reports().OpenAsync(client, ReportKind.Review, reviewId))!.Access);
        var review = await Reports().OpenAsync(accountant, ReportKind.Review, reviewId);
        Assert.Equal(ReportAccess.Allowed, review!.Access); // The reviewed accountant may report a review of them.
        Assert.Equal("Client P.'s review of Nina Accountant", review.Target!.Description);

        var visitor = await Reports().OpenAsync(null, ReportKind.Conversation, conversation);
        Assert.Equal(ReportAccess.SignIn, visitor!.Access);
        Assert.Null(visitor.Target); // Nothing about a private conversation.
        Assert.Equal(ReportOutcome.NotAllowed, (await Reports().SendAsync(null, ReportKind.Profile, profile.Value, Abusive)).Outcome);
    }

    [DatabaseFact]
    public async Task Only_participants_report_a_conversation_and_unknown_items_are_not_available()
    {
        var (client, accountant, _, conversation) = await PairAsync(replied: true);
        var outsider = new ChatViewer(await AccountAsync("Outside Person", AccountRole.Client), AccountRole.Client);

        var page = await Reports().OpenAsync(accountant, ReportKind.Conversation, conversation);
        Assert.Equal("Your conversation with Client Person", page!.Target!.Description);
        Assert.Equal("Your conversation with Nina Accountant", (await Reports().OpenAsync(client, ReportKind.Conversation, conversation))!.Target!.Description);
        Assert.Null(await Reports().OpenAsync(outsider, ReportKind.Conversation, conversation));
        Assert.Equal(ReportOutcome.Gone, (await Reports().SendAsync(outsider, ReportKind.Conversation, conversation, Abusive)).Outcome);
        Assert.Null(await Reports().OpenAsync(client, ReportKind.Profile, Guid.NewGuid()));
        Assert.Null(await Reports().OpenAsync(null, ReportKind.Review, Guid.NewGuid()));

        var sent = await Reports().SendAsync(accountant, ReportKind.Conversation, conversation, Abusive);
        Assert.Equal($"/chats/{conversation:D}?reported=1", sent.ReturnPath);
    }

    [DatabaseFact]
    public async Task An_invalid_report_stores_nothing_and_ten_a_day_is_the_most()
    {
        var reporter = new ChatViewer(await AccountAsync("Busy Reporter", AccountRole.Client), AccountRole.Client);
        var profiles = new List<AccountantId>();
        for (var i = 0; i < ReportLimits.PerDay + 1; i++)
        {
            profiles.Add((await PairAsync()).Profile);
        }

        Assert.Equal(ReportOutcome.Invalid, (await Reports().SendAsync(reporter, ReportKind.Profile, profiles[0].Value, new ReportForm())).Outcome);
        foreach (var profile in profiles.Take(ReportLimits.PerDay))
        {
            Assert.Equal(ReportOutcome.Sent, (await Reports().SendAsync(reporter, ReportKind.Profile, profile.Value, Abusive)).Outcome);
        }

        Assert.Equal(ReportOutcome.TooMany, (await Reports().SendAsync(reporter, ReportKind.Profile, profiles[^1].Value, Abusive)).Outcome);
        Assert.Equal(ReportOutcome.Sent, (await Reports(Now.AddDays(1).AddMinutes(1)).SendAsync(reporter, ReportKind.Profile, profiles[^1].Value, Abusive)).Outcome);
    }

    [DatabaseFact]
    public async Task While_either_side_has_blocked_neither_can_send_and_unblocking_restores_it()
    {
        var (client, accountant, profile, conversation) = await PairAsync(replied: true);

        Assert.True(await Chat.SetBlockAsync(accountant.AccountId, conversation, blocked: true));
        Assert.Equal(SendOutcome.Blocked, (await Chat.SendAsync(client, conversation, null, Guid.NewGuid(), "Hello?")).Outcome);
        Assert.Equal(SendOutcome.Blocked, (await Chat.SendAsync(client, null, profile, Guid.NewGuid(), "From the profile")).Outcome);
        Assert.Equal(SendOutcome.Blocked, (await Chat.SendAsync(accountant, conversation, null, Guid.NewGuid(), "Me too")).Outcome);

        var clientView = await Chat.OpenThreadAsync(client.AccountId, conversation);
        var accountantView = await Chat.OpenThreadAsync(accountant.AccountId, conversation);
        Assert.Equal((false, true), (clientView!.BlockedByMe, clientView.BlockedByThem));
        Assert.Equal((true, false), (accountantView!.BlockedByMe, accountantView.BlockedByThem));
        Assert.Equal(2, clientView.Messages.Count); // Nothing new was stored; the old messages stay readable.

        Assert.False(await Chat.SetBlockAsync(client.AccountId, Guid.NewGuid(), blocked: true));
        var outsider = await AccountAsync("Outside Person", AccountRole.Client);
        Assert.False(await Chat.SetBlockAsync(outsider, conversation, blocked: true));
        Assert.True(await Chat.SetBlockAsync(client.AccountId, conversation, blocked: false)); // Not the client's block to lift.
        Assert.Equal(SendOutcome.Blocked, (await Chat.SendAsync(client, conversation, null, Guid.NewGuid(), "Still?")).Outcome);

        Assert.True(await Chat.SetBlockAsync(accountant.AccountId, conversation, blocked: false));
        Assert.Equal(SendOutcome.Sent, (await Chat.SendAsync(client, conversation, null, Guid.NewGuid(), "Thanks for unblocking.")).Outcome);
    }

    [DatabaseFact]
    public async Task A_client_the_accountant_blocked_cannot_review_but_can_delete_their_review()
    {
        var (client, accountant, profile, conversation) = await PairAsync(replied: true);
        Assert.Equal(ReviewOutcome.Posted, (await Reviews.SaveAsync(client, profile, new ReviewForm { Stars = "2", Text = "Slow." })).Outcome);

        await Chat.SetBlockAsync(accountant.AccountId, conversation, blocked: true);

        var page = await Reviews.OpenFormAsync(client, profile);
        Assert.Equal(ReviewAccess.Blocked, page!.Access);
        Assert.NotNull(page.Saved); // Shown so it can be deleted.
        Assert.Equal(ReviewAccess.Blocked, (await Reviews.ForProfileAsync(client, profile)).Access);
        var refused = await Reviews.SaveAsync(client, profile, new ReviewForm { Stars = "1" });
        Assert.Equal((ReviewOutcome.NotAllowed, ReviewAccess.Blocked), (refused.Outcome, refused.Access));
        Assert.Single((await Reviews.ForProfileAsync(null, profile)).Reviews); // The review stays visible.
        Assert.Equal(ReviewOutcome.Deleted, (await Reviews.DeleteAsync(client, profile)).Outcome);
    }

    [DatabaseFact]
    public async Task A_block_set_by_the_client_does_not_change_their_own_review_rights()
    {
        var (client, _, profile, conversation) = await PairAsync(replied: true);

        await Chat.SetBlockAsync(client.AccountId, conversation, blocked: true);

        Assert.Equal(ReviewAccess.Allowed, (await Reviews.OpenFormAsync(client, profile))!.Access);
    }

    [DatabaseFact]
    public async Task A_failed_report_reports_failed()
    {
        var reports = new ReportService(new ReportStore(new UnreachableSchema(), NullLogger<ReportStore>.Instance), new FixedClock(Now),
            NullLogger<ReportService>.Instance);

        var result = await reports.SendAsync(new ChatViewer(Guid.NewGuid(), AccountRole.Client), ReportKind.Profile, Guid.NewGuid(), Abusive);

        Assert.Equal(ReportOutcome.Failed, result.Outcome);
    }

    private async Task<(ChatViewer Client, ChatViewer Accountant, AccountantId Profile, Guid Conversation)> PairAsync(bool replied = false)
    {
        var clientId = await AccountAsync("Client Person", AccountRole.Client);
        var accountantId = await AccountAsync("Nina Account Name", AccountRole.Accountant);
        var profile = AccountantProfile.CreateFor(accountantId, "Nina Accountant", "nina@example.test");
        profile.Edit("Nina Accountant", "Certified Public Accountant", "Taguig City", "Synthetic summary.", "nina@example.test", null, null, true);
        profile.ReplaceServices([("Bookkeeping", "")]);
        profile.MarkSaved();
        await fixture.Schema.AddAsync(profile);
        var client = new ChatViewer(clientId, AccountRole.Client);
        var accountant = new ChatViewer(accountantId, AccountRole.Accountant);
        var conversation = Guid.Empty;
        if (replied)
        {
            conversation = (await Chat.SendAsync(client, null, profile.Id, Guid.NewGuid(), "Hi! Can you help?")).ConversationId!.Value;
            await Chat.SendAsync(accountant, conversation, null, Guid.NewGuid(), "Yes, happy to.");
        }

        return (client, accountant, profile.Id, conversation);
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
    private sealed class UnreachableSchema : IDbContextFactory<TaxBuddyDbContext>
    {
        public TaxBuddyDbContext CreateDbContext() => new(new DbContextOptionsBuilder<TaxBuddyDbContext>()
            .UseNpgsql("Host=127.0.0.1;Port=1;Database=none;Username=none;Password=none;Timeout=3").Options);
    }
}
