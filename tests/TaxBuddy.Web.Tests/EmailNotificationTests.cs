using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaxBuddy.TestSupport;
using TaxBuddy.Web.Account;
using TaxBuddy.Web.Application.Chat;
using TaxBuddy.Web.Domain;
using TaxBuddy.Web.Services.Email;
using TaxBuddy.Web.Tests.Support;
using static TaxBuddy.Web.Tests.Support.FormPosts;

namespace TaxBuddy.Web.Tests;

/// <summary>
/// F14 new-message emails through the whole app (EM01–EM08): one job per conversation until it is opened, the outbox's
/// send, skip and retry, the email's content, and turning emails off from the link or Account. The tests run the outbox
/// themselves and make jobs due by moving their due time.
/// </summary>
public sealed class EmailNotificationTests(EmptyAppFixture app) : IClassFixture<EmptyAppFixture>
{
    [DatabaseFact]
    public async Task A_message_queues_one_email_until_the_recipient_opens_the_conversation()
    {
        var (accountant, client, conversation) = await ChatAsync();

        await SendAsync(client, null, conversation, "Are you free next week?");
        Assert.Equal(1, await JobsAsync(accountant.Id, conversation));

        await OpenAsync(accountant.Id, conversation);
        await SendAsync(client, null, conversation, "One more question.");
        Assert.Equal(2, await JobsAsync(accountant.Id, conversation));

        await SendAsync(new ChatViewer(accountant.Id, AccountRole.Accountant), null, conversation, "Yes.");
        Assert.Equal(1, await JobsAsync(client.AccountId, conversation)); // The other way, for the client.
    }

    [DatabaseFact]
    public async Task A_due_email_is_sent_with_both_links_and_never_the_message_text()
    {
        var (accountant, _, conversation) = await ChatAsync("Please check my VAT return for March.");
        await MakeDueAsync(conversation);

        await RunOutboxAsync();

        var email = Assert.Single(app.Factory.Emails.Sent, e => e.To == accountant.Email);
        Assert.Equal("New message from Olive on Tax Buddy", email.Subject);
        Assert.Contains("Hi Nina,", email.Text);
        Assert.Contains($"https://taxbuddy.test/chats/{conversation:D}", email.Text);
        Assert.Contains("https://taxbuddy.test/email/off?t=", email.Text);
        Assert.DoesNotContain("VAT", email.Text + email.Html);
        Assert.Equal(EmailJobStatus.Sent, await StatusAsync(conversation, accountant.Id));
        Assert.DoesNotContain(app.Factory.Logs.Messages, m => m.Contains(accountant.Email, StringComparison.OrdinalIgnoreCase));
    }

    [DatabaseFact]
    public async Task No_email_goes_when_the_recipient_read_it_first_or_turned_emails_off()
    {
        var (readFirst, _, read) = await ChatAsync();
        await OpenAsync(readFirst.Id, read);
        var (turnedOff, _, off) = await ChatAsync();
        using (var scope = app.Factory.Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<EmailPreferences>().SetAsync(turnedOff.Id, false);
        }

        await MakeDueAsync(read);
        await MakeDueAsync(off);
        await RunOutboxAsync();

        Assert.Equal(EmailJobStatus.Skipped, await StatusAsync(read, readFirst.Id));
        Assert.Equal(EmailJobStatus.Skipped, await StatusAsync(off, turnedOff.Id));
        Assert.DoesNotContain(app.Factory.Emails.Sent, e => e.To == readFirst.Email || e.To == turnedOff.Email);
    }

    [DatabaseFact]
    public async Task A_failed_send_waits_and_tries_again_and_the_message_stays()
    {
        var (accountant, _, conversation) = await ChatAsync();
        await MakeDueAsync(conversation);
        app.Factory.Emails.Fail = true;
        try
        {
            await RunOutboxAsync();
        }
        finally
        {
            app.Factory.Emails.Fail = false;
        }

        await using var db = app.Schema.CreateContext();
        var job = await db.EmailJobs.SingleAsync(j => j.ConversationId == conversation && j.RecipientAccountId == accountant.Id);
        Assert.Equal((EmailJobStatus.Pending, 1), (job.Status, job.Attempts));
        Assert.True(job.DueAt > DateTimeOffset.UtcNow);
        Assert.True(await db.Messages.AnyAsync(m => m.ConversationId == conversation));
    }

    [DatabaseFact]
    public async Task The_email_link_turns_emails_off_without_signing_in_and_they_can_go_back_on()
    {
        var (accountant, _, _) = await ChatAsync();
        var path = app.Factory.Services.GetRequiredService<EmailLinks>().TurnOffPath(accountant.Id);
        var visitor = app.Factory.CreateHttpsClient();

        var page = await visitor.GetHtmlAsync(path);
        Assert.Contains("Notification emails are off", page);
        Assert.Contains("We won't email you about new messages. Unread messages still show in Chats.", page);
        Assert.DoesNotContain(accountant.Email, page); // A forwarded link doesn't show the address.
        Assert.False(await EmailsOnAsync(accountant.Id));

        var fields = HiddenFields(page, "Notification emails are off");
        var back = await visitor.PostAsync(EmailPreferences.EndpointPath, new FormUrlEncodedContent(fields));
        Assert.Equal(HttpStatusCode.Redirect, back.StatusCode);
        Assert.True(await EmailsOnAsync(accountant.Id));
        Assert.Contains("Notification emails are on", await visitor.GetHtmlAsync(Target(back)!)); // Coming back doesn't turn them off again.
        Assert.True(await EmailsOnAsync(accountant.Id));
    }

    [DatabaseFact]
    public async Task Account_shows_the_setting_and_turns_it_off_and_on()
    {
        var (accountant, _, _) = await ChatAsync();

        var account = await accountant.Client.GetHtmlAsync("/account");
        Assert.Contains("Email notifications: On", account);
        Assert.Contains($"We email {accountant.Email} when a client messages you and you're not on the site.", account);

        var off = await accountant.Client.PostAsync(EmailPreferences.EndpointPath, new FormUrlEncodedContent(HiddenFields(account, "Email notifications: On")));
        Assert.Equal("/account", Target(off));
        Assert.False(await EmailsOnAsync(accountant.Id));
        var after = await accountant.Client.GetHtmlAsync("/account");
        Assert.Contains("Email notifications: Off", after);
        Assert.Contains("You won't get emails. New messages still show as unread in Chats.", after);
    }

    [DatabaseFact]
    public async Task A_broken_link_changes_nothing()
    {
        var visitor = app.Factory.CreateHttpsClient();

        var page = await visitor.GetHtmlAsync("/email/off?t=not-a-real-token");
        Assert.Contains("This link doesn't work any more", page);

        var forged = await visitor.PostAsync(EmailPreferences.EndpointPath, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = Token(page), ["on"] = "false", ["t"] = "not-a-real-token", ["returnUrl"] = "/",
        }));
        Assert.Equal(HttpStatusCode.BadRequest, forged.StatusCode);
    }

    [DatabaseFact]
    public async Task Deleting_an_account_drops_its_pending_emails()
    {
        var (accountant, _, conversation) = await ChatAsync();
        var confirm = await accountant.Client.GetHtmlAsync("/account/delete?step=confirm");
        await accountant.Client.PostAsync("/account/delete?step=confirm", new FormUrlEncodedContent(HiddenFields(confirm, "Are you sure?")));

        Assert.Equal(0, await JobsAsync(accountant.Id, conversation));
    }

    private sealed record Person(HttpClient Client, Guid Id, string Email);

    /// <summary>A listed accountant, a client and the client's first message: one email job for the accountant.</summary>
    private async Task<(Person Accountant, ChatViewer Client, Guid Conversation)> ChatAsync(string firstMessage = "Hi! Can you help?")
    {
        var accountant = await RegisterAsync(AccountRole.Accountant, "Nina Ocampo");
        var profile = AccountantProfile.CreateFor(accountant.Id, "Nina Ocampo", "nina@example.test");
        profile.Edit("Nina Ocampo", "Certified Public Accountant", "Taguig City", "Synthetic summary.", "nina@example.test", null, null, true);
        profile.ReplaceServices([("Bookkeeping", "")]);
        profile.MarkSaved();
        await using (var db = app.Schema.CreateContext())
        {
            db.AccountantProfiles.Add(profile);
            await db.SaveChangesAsync();
        }

        var client = await RegisterAsync(AccountRole.Client, "Olive Reyes");
        var viewer = new ChatViewer(client.Id, AccountRole.Client);
        var conversation = await SendAsync(viewer, profile.Id, null, firstMessage);
        return (accountant, viewer, conversation);
    }

    private async Task<Person> RegisterAsync(AccountRole role, string name)
    {
        var email = $"{Guid.NewGuid():N}@example.test";
        var client = app.Factory.CreateHttpsClient();
        await TestAccounts.RegisterAsync(client, role, name, email);
        await using var db = app.Schema.CreateContext();
        return new Person(client, (await db.Users.SingleAsync(u => u.Email == email)).Id, email);
    }

    private async Task<Guid> SendAsync(ChatViewer sender, AccountantId? newChatWith, Guid? conversation, string text)
    {
        using var scope = app.Factory.Services.CreateScope();
        var sent = await scope.ServiceProvider.GetRequiredService<ChatService>().SendAsync(sender, conversation, newChatWith, Guid.NewGuid(), text);
        Assert.Equal(SendOutcome.Sent, sent.Outcome);
        return sent.ConversationId!.Value;
    }

    private async Task OpenAsync(Guid accountId, Guid conversation)
    {
        using var scope = app.Factory.Services.CreateScope();
        Assert.NotNull(await scope.ServiceProvider.GetRequiredService<ChatService>().OpenThreadAsync(accountId, conversation));
    }

    private async Task RunOutboxAsync()
    {
        using var scope = app.Factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<EmailOutbox>().ProcessDueAsync();
    }

    private async Task MakeDueAsync(Guid conversation)
    {
        await using var db = app.Schema.CreateContext();
        await db.EmailJobs.Where(j => j.ConversationId == conversation)
            .ExecuteUpdateAsync(s => s.SetProperty(j => j.DueAt, DateTimeOffset.UtcNow.AddMinutes(-1)));
    }

    private async Task<int> JobsAsync(Guid recipient, Guid conversation)
    {
        await using var db = app.Schema.CreateContext();
        return await db.EmailJobs.CountAsync(j => j.RecipientAccountId == recipient && j.ConversationId == conversation);
    }

    private async Task<EmailJobStatus> StatusAsync(Guid conversation, Guid recipient)
    {
        await using var db = app.Schema.CreateContext();
        return (await db.EmailJobs.SingleAsync(j => j.ConversationId == conversation && j.RecipientAccountId == recipient)).Status;
    }

    private async Task<bool> EmailsOnAsync(Guid accountId)
    {
        await using var db = app.Schema.CreateContext();
        return (await db.Users.SingleAsync(u => u.Id == accountId)).EmailNotifications;
    }
}
