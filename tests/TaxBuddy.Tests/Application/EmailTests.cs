using Microsoft.Extensions.Options;
using TaxBuddy.Web.Application.Email;
using TaxBuddy.Web.Domain;
using TaxBuddy.Web.Services;
using TaxBuddy.Web.Services.Email;

namespace TaxBuddy.Tests.Application;

/// <summary>F14 rules without a database: when a new email is due (D18), retries, the email's words, the development sender.</summary>
public sealed class EmailTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 23, 4, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(null, null, true)]      // No email yet for this conversation.
    [InlineData(null, 0, false)]        // Emailed, never opened since.
    [InlineData(-5, 0, false)]          // Opened before the last email, not after.
    [InlineData(0, 0, true)]            // Opened up to the message that was emailed.
    [InlineData(10, 0, true)]           // Opened after it.
    public void One_email_per_conversation_until_it_is_opened(int? readMinutes, int? jobMinutes, bool expected) =>
        Assert.Equal(expected, EmailRules.NeedsNewJob(
            readMinutes is { } r ? Now.AddMinutes(r) : null, jobMinutes is { } j ? Now.AddMinutes(j) : null));

    [Fact]
    public void A_job_waits_then_retries_with_longer_waits_and_gives_up_after_six_tries()
    {
        var job = new EmailJob(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Now);
        Assert.Equal(Now + EmailRules.Delay, job.DueAt);

        job.MarkAttemptFailed(Now);
        Assert.Equal((EmailJobStatus.Pending, Now.AddMinutes(1)), (job.Status, job.DueAt));
        for (var i = 2; i <= EmailRules.MaxAttempts; i++)
        {
            job.MarkAttemptFailed(Now);
        }

        Assert.Equal((EmailJobStatus.Failed, EmailRules.MaxAttempts), (job.Status, job.Attempts));
        Assert.Equal(Now, job.FinishedAt);
    }

    [Fact]
    public void The_email_names_the_sender_and_links_but_holds_no_message_text()
    {
        var email = EmailOutbox.Compose("nina@example.test", "Nina Ocampo", "Olive <Reyes>", "https://site/chats/1", "https://site/email/off?t=x&y");

        Assert.Equal("New message from Olive on Tax Buddy", email.Subject);
        Assert.Contains("Hi Nina,", email.Text);
        Assert.Contains("Open the conversation: https://site/chats/1", email.Text);
        Assert.Contains("Turn them off: https://site/email/off?t=x&y", email.Text);
        Assert.Contains("href=\"https://site/email/off?t=x&amp;y\"", email.Html); // Encoded in HTML.
    }

    [Fact]
    public async Task The_development_sender_writes_each_email_to_the_folder()
    {
        var folder = Path.Combine(Path.GetTempPath(), "taxbuddy-email-test-" + Guid.NewGuid().ToString("N"));
        try
        {
            var sender = new DevelopmentEmailSender(Options.Create(new EmailOptions { DevelopmentFolder = folder }), new FixedClock(Now));

            await sender.SendAsync(new EmailMessage("nina@example.test", "Subject", "Text body", "<p>Html</p>"), CancellationToken.None);

            var text = Assert.Single(Directory.GetFiles(folder, "*.txt"));
            Assert.Contains("To: nina@example.test\nSubject: Subject\n\nText body", await File.ReadAllTextAsync(text));
            Assert.Single(Directory.GetFiles(folder, "*.html"));
        }
        finally
        {
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, recursive: true);
            }
        }
    }

    private sealed class FixedClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow => now;
    }
}
