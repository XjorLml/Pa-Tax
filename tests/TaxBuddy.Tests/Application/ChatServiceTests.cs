using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TaxBuddy.TestSupport;
using TaxBuddy.Web.Application.Chat;
using TaxBuddy.Web.Data;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Tests.Application;

/// <summary>The message text rule (PRD BR04, D06).</summary>
public sealed class MessageTextTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\r\n\t ")]
    [InlineData(null)]
    public void Empty_or_whitespace_is_refused(string? text) =>
        Assert.Equal(MessageProblem.Empty, MessageText.Normalize(text).Problem);

    [Fact]
    public void Up_to_2000_characters_after_trimming_is_allowed_and_one_more_is_not()
    {
        Assert.Equal(2000, MessageText.Normalize("  " + new string('a', 2000) + "\n").Text?.Length);
        Assert.Equal(MessageProblem.TooLong, MessageText.Normalize(new string('a', 2001)).Problem);
    }

    [Fact]
    public void Text_is_trimmed_and_line_endings_become_newlines() =>
        Assert.Equal("Hi!\nI need help.", MessageText.Normalize("  Hi!\r\nI need help.\r\n ").Text);
}

/// <summary>
/// F07 chat rules on the real PostgreSQL provider, in a throwaway schema (CH01–CH04, CH07–CH09; BR03–BR05). Each test
/// creates its own accounts, so the tests share the schema without seeing each other's conversations.
/// </summary>
public sealed class ChatServiceTests(SchemaFixture fixture) : IClassFixture<SchemaFixture>
{
    private static readonly DateTimeOffset Now = new(2026, 11, 16, 6, 0, 0, TimeSpan.Zero);

    private readonly RecordingNotifier notifier = new();

    private ChatStore Store => new(fixture.Schema, NullLogger<ChatStore>.Instance);

    private ChatService Service(DateTimeOffset? at = null) => new(Store, notifier, new FixedClock(at ?? Now));

    [DatabaseFact]
    public async Task A_clients_first_message_creates_the_conversation_and_the_accountant_sees_it_as_unread()
    {
        var (client, accountant, profile) = await PairAsync();
        var chat = Service();

        var prepared = await chat.PrepareNewChatAsync(client, profile);
        Assert.Equal(NewChatOutcome.Ready, prepared.Outcome);
        Assert.Equal(accountant.AccountId, prepared.Recipient?.AccountId);

        var sent = await chat.SendAsync(client, null, profile, Guid.NewGuid(), "  Hi! I need help with bookkeeping. ");

        Assert.Equal(SendOutcome.Sent, sent.Outcome);
        Assert.Equal("Hi! I need help with bookkeeping.", sent.Message?.Text);
        var row = Assert.Single(await chat.ListAsync(accountant.AccountId));
        Assert.Equal(sent.ConversationId, row.Id);
        Assert.Equal("Client Person", row.PeerName);
        Assert.Equal(1, row.Unread);
        Assert.False(row.LastIsMine);
        Assert.Equal(1, await chat.UnreadTotalAsync(accountant.AccountId));
        Assert.Equal(0, await chat.UnreadTotalAsync(client.AccountId)); // Your own message is never unread.
        Assert.Equal("Nina Accountant", Assert.Single(await chat.ListAsync(client.AccountId)).PeerName); // The profile's name.
        Assert.Contains(sent.ConversationId!.Value, notifier.Changed);
    }

    [DatabaseFact]
    public async Task There_is_one_conversation_per_client_and_accountant()
    {
        var (client, _, profile) = await PairAsync();
        var chat = Service();
        var first = await chat.SendAsync(client, null, profile, Guid.NewGuid(), "First");

        var again = await chat.PrepareNewChatAsync(client, profile);
        var second = await Service(Now.AddSeconds(5)).SendAsync(client, null, profile, Guid.NewGuid(), "Second, from the profile again");

        Assert.Equal(NewChatOutcome.Existing, again.Outcome);
        Assert.Equal(first.ConversationId, again.ConversationId);
        Assert.Equal(first.ConversationId, second.ConversationId);
        var thread = await chat.OpenThreadAsync(client.AccountId, first.ConversationId!.Value);
        Assert.Equal(["First", "Second, from the profile again"], thread!.Messages.Select(m => m.Text));
    }

    [DatabaseFact]
    public async Task Sending_the_same_message_twice_stores_it_once()
    {
        var (client, accountant, profile) = await PairAsync();
        var chat = Service();
        var id = Guid.NewGuid();

        var first = await chat.SendAsync(client, null, profile, id, "Hello");
        var retry = await chat.SendAsync(client, first.ConversationId, null, id, "Hello");

        Assert.Equal(SendOutcome.Sent, retry.Outcome);
        Assert.Equal(first.Message?.Id, retry.Message?.Id);
        Assert.Single((await chat.OpenThreadAsync(accountant.AccountId, first.ConversationId!.Value))!.Messages);
        Assert.True(await chat.IsStoredAsync(client.AccountId, id));
        Assert.False(await chat.IsStoredAsync(client.AccountId, Guid.NewGuid()));
    }

    [DatabaseFact]
    public async Task Opening_a_thread_marks_it_read_and_a_reply_is_unread_for_the_client()
    {
        var (client, accountant, profile) = await PairAsync();
        var conversation = (await Service().SendAsync(client, null, profile, Guid.NewGuid(), "Question")).ConversationId!.Value;

        var opened = await Service(Now.AddMinutes(1)).OpenThreadAsync(accountant.AccountId, conversation);
        Assert.False(opened!.ViewerIsClient);
        Assert.Null(opened.PeerProfileId); // A client has no public profile.
        Assert.Equal(0, await Service().UnreadTotalAsync(accountant.AccountId));

        var reply = await Service(Now.AddMinutes(2)).SendAsync(accountant, conversation, null, Guid.NewGuid(), "Answer");
        Assert.Equal(SendOutcome.Sent, reply.Outcome);
        Assert.Equal(1, Assert.Single(await Service().ListAsync(client.AccountId)).Unread);

        var clientView = await Service(Now.AddMinutes(3)).OpenThreadAsync(client.AccountId, conversation);
        Assert.True(clientView!.ViewerIsClient);
        Assert.Equal(profile, clientView.PeerProfileId);
        Assert.Equal([true, false], clientView.Messages.Select(m => m.Mine));
        Assert.Equal(0, await Service().UnreadTotalAsync(client.AccountId));
    }

    [DatabaseFact]
    public async Task Nobody_outside_the_conversation_can_read_it_or_write_to_it()
    {
        var (client, _, profile) = await PairAsync();
        var outsider = new ChatViewer(await AccountAsync("Other Client", AccountRole.Client), AccountRole.Client);
        var message = Guid.NewGuid();
        var conversation = (await Service().SendAsync(client, null, profile, message, "Private")).ConversationId!.Value;

        Assert.Null(await Service().OpenThreadAsync(outsider.AccountId, conversation));
        Assert.Null(await Service().OpenThreadAsync(outsider.AccountId, Guid.NewGuid()));
        Assert.Equal(SendOutcome.NotAllowed, (await Service().SendAsync(outsider, conversation, null, Guid.NewGuid(), "Hi")).Outcome);
        // Reusing someone else's message ID is refused, and reveals nothing.
        var forged = await Service().SendAsync(outsider, null, profile, message, "Private");
        Assert.Equal(SendOutcome.NotAllowed, forged.Outcome);
        Assert.Null(forged.Message);
        Assert.Empty(await Service().ListAsync(outsider.AccountId));
    }

    [DatabaseFact]
    public async Task Only_clients_start_chats_and_only_with_listed_accountants_who_have_an_account()
    {
        var (client, accountant, profile) = await PairAsync();
        var seedProfile = Profiles.Listed("Seed Accountant");
        var unlisted = Profiles.WithoutConsent("Unlisted Accountant");
        await fixture.Schema.AddAsync(seedProfile, unlisted);

        Assert.Equal(NewChatOutcome.NotAClient, (await Service().PrepareNewChatAsync(accountant, profile)).Outcome);
        Assert.Equal(NewChatOutcome.NoAccount, (await Service().PrepareNewChatAsync(client, seedProfile.Id)).Outcome);
        Assert.Equal(NewChatOutcome.Unavailable, (await Service().PrepareNewChatAsync(client, unlisted.Id)).Outcome);
        Assert.Equal(NewChatOutcome.Unavailable, (await Service().PrepareNewChatAsync(client, AccountantId.New())).Outcome);
        Assert.Equal(NewChatOutcome.Ready, (await Service().PrepareNewChatAsync(null, profile)).Outcome); // A visitor may type first.
        Assert.Equal(SendOutcome.NotAllowed, (await Service().SendAsync(accountant, null, profile, Guid.NewGuid(), "Hi")).Outcome);
        Assert.Equal(SendOutcome.NotAllowed, (await Service().SendAsync(client, null, seedProfile.Id, Guid.NewGuid(), "Hi")).Outcome);
        Assert.Empty(await Service().ListAsync(client.AccountId));
    }

    [DatabaseFact]
    public async Task Invalid_text_is_refused_and_nothing_is_stored()
    {
        var (client, _, profile) = await PairAsync();

        var empty = await Service().SendAsync(client, null, profile, Guid.NewGuid(), "  \n ");
        var tooLong = await Service().SendAsync(client, null, profile, Guid.NewGuid(), new string('x', ChatLimits.MessageLength + 1));

        Assert.Equal((SendOutcome.Invalid, MessageProblem.Empty), (empty.Outcome, empty.Problem));
        Assert.Equal((SendOutcome.Invalid, MessageProblem.TooLong), (tooLong.Outcome, tooLong.Problem));
        Assert.Empty(await Service().ListAsync(client.AccountId));
    }

    [DatabaseFact]
    public async Task More_than_20_messages_in_a_minute_are_refused()
    {
        var (client, accountant, profile) = await PairAsync();
        var conversation = (await Service().SendAsync(client, null, profile, Guid.NewGuid(), "1")).ConversationId!.Value;
        for (var i = 2; i <= ChatLimits.MessagesPerMinute; i++)
        {
            Assert.Equal(SendOutcome.Sent, (await Service().SendAsync(client, conversation, null, Guid.NewGuid(), $"{i}")).Outcome);
        }

        var tooFast = await Service().SendAsync(client, conversation, null, Guid.NewGuid(), "21");
        var aMinuteLater = await Service(Now.AddSeconds(61)).SendAsync(client, conversation, null, Guid.NewGuid(), "21");

        Assert.Equal(SendOutcome.TooFast, tooFast.Outcome);
        Assert.Equal(SendOutcome.Sent, aMinuteLater.Outcome);
        Assert.Equal(21, (await Service().OpenThreadAsync(accountant.AccountId, conversation))!.Messages.Count);
    }

    [DatabaseFact]
    public async Task An_accountant_sees_the_pictures_of_clients_they_chat_with_and_no_others()
    {
        var (client, accountant, profile) = await PairAsync();
        var otherAccountant = await AccountAsync("Other Accountant", AccountRole.Accountant);
        var pictures = new AccountPictureStore(fixture.Schema, NullLogger<AccountPictureStore>.Instance);
        Assert.False(await pictures.SharesConversationAsync(client.AccountId, accountant.AccountId, default));

        await Service().SendAsync(client, null, profile, Guid.NewGuid(), "Hello");

        Assert.True(await pictures.SharesConversationAsync(client.AccountId, accountant.AccountId, default));
        Assert.False(await pictures.SharesConversationAsync(client.AccountId, otherAccountant, default));
        Assert.False(await pictures.SharesConversationAsync(accountant.AccountId, client.AccountId, default));
    }

    [Fact]
    public async Task A_failed_save_reports_failed_and_the_check_says_unknown()
    {
        var unreachable = new UnreachableSchema();
        var chat = new ChatService(new ChatStore(unreachable, NullLogger<ChatStore>.Instance), notifier, new FixedClock(Now));
        var client = new ChatViewer(Guid.NewGuid(), AccountRole.Client);

        var sent = await chat.SendAsync(client, Guid.NewGuid(), null, Guid.NewGuid(), "Hello");

        Assert.Equal(SendOutcome.Failed, sent.Outcome);
        Assert.Null(await chat.IsStoredAsync(client.AccountId, Guid.NewGuid()));
        Assert.Empty(notifier.Changed);
    }

    private async Task<(ChatViewer Client, ChatViewer Accountant, AccountantId Profile)> PairAsync()
    {
        var clientId = await AccountAsync("Client Person", AccountRole.Client);
        var accountantId = await AccountAsync("Nina Account Name", AccountRole.Accountant);
        var profile = AccountantProfile.CreateFor(accountantId, "Nina Accountant", "nina@example.test");
        profile.Edit("Nina Accountant", "Certified Public Accountant", "Taguig City", "Synthetic summary.", "nina@example.test", null, null, true);
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

    private sealed class RecordingNotifier : IChatNotifier
    {
        public List<Guid> Changed { get; } = [];

        public IDisposable Subscribe(Guid accountId, Func<Guid, Task> changed) => throw new NotSupportedException();

        void IChatNotifier.Changed(Conversation conversation) => Changed.Add(conversation.Id);
    }

    /// <summary>A database where nothing listens, so every call fails at once.</summary>
    private sealed class UnreachableSchema : Microsoft.EntityFrameworkCore.IDbContextFactory<TaxBuddyDbContext>
    {
        public TaxBuddyDbContext CreateDbContext() => new(new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<TaxBuddyDbContext>()
            .UseNpgsql("Host=127.0.0.1;Port=1;Database=none;Username=none;Password=none;Timeout=3").Options);
    }
}
