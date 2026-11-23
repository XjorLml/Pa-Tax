using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TaxBuddy.Web.Application.Email;
using TaxBuddy.Web.Data;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Services.Email;

/// <summary>
/// Sends the notification emails that are due (F14; SDD email contract). A job is skipped when the recipient has read the
/// conversation since the message, turned emails off, deleted their account, or the site has no email service. A failed
/// send waits and tries again, then gives up; the message itself is never affected. Logs carry no addresses or names.
/// </summary>
public sealed class EmailOutbox(
    IDbContextFactory<TaxBuddyDbContext> contexts,
    IEmailSender sender,
    EmailLinks links,
    IClock clock,
    ILogger<EmailOutbox> logger)
{
    public const int BatchSize = 20;

    private static readonly EventId Sent = new(1501, "NotificationEmailSent");
    private static readonly EventId AttemptFailed = new(1502, "NotificationEmailFailed");
    private static readonly EventId GaveUp = new(1503, "NotificationEmailGaveUp");
    private static readonly EventId NotConfigured = new(1504, "EmailNotConfigured");

    /// <summary>Handles up to <see cref="BatchSize"/> due jobs and returns how many it handled.</summary>
    public async Task<int> ProcessDueAsync(CancellationToken cancellationToken = default)
    {
        var now = clock.UtcNow;
        await using var db = await contexts.CreateDbContextAsync(cancellationToken);
        var keepFrom = now - EmailRules.KeepFinished;
        await db.EmailJobs.Where(j => j.Status != EmailJobStatus.Pending && j.FinishedAt < keepFrom).ExecuteDeleteAsync(cancellationToken);

        var jobs = await db.EmailJobs
            .Where(j => j.Status == EmailJobStatus.Pending && j.DueAt <= now)
            .OrderBy(j => j.DueAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);
        if (jobs.Count > 0 && !sender.IsConfigured)
        {
            logger.LogWarning(NotConfigured, "Notification emails skipped: no email service is set up ({Count})", jobs.Count);
        }

        foreach (var job in jobs)
        {
            await HandleAsync(db, job, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }

        return jobs.Count;
    }

    private async Task HandleAsync(TaxBuddyDbContext db, EmailJob job, CancellationToken cancellationToken)
    {
        var conversation = await db.Conversations.AsNoTracking().FirstOrDefaultAsync(c => c.Id == job.ConversationId, cancellationToken);
        var recipient = await db.Users.AsNoTracking().Where(u => u.Id == job.RecipientAccountId)
            .Select(u => new { u.Email, u.DisplayName, u.EmailNotifications, u.DeletedAt })
            .FirstOrDefaultAsync(cancellationToken);
        if (!sender.IsConfigured
            || conversation is null
            || recipient is null || recipient.DeletedAt is not null || !recipient.EmailNotifications || string.IsNullOrWhiteSpace(recipient.Email)
            || conversation.ReadAtFor(job.RecipientAccountId) >= job.CreatedAt)
        {
            job.MarkSkipped(clock.UtcNow);
            return;
        }

        var senderId = conversation.OtherThan(job.RecipientAccountId);
        var senderName = await db.AccountantProfiles.Where(p => p.OwnerAccountId == senderId).Select(p => p.DisplayName).FirstOrDefaultAsync(cancellationToken)
                         ?? await db.Users.Where(u => u.Id == senderId).Select(u => u.DisplayName).FirstOrDefaultAsync(cancellationToken)
                         ?? "Someone";
        var message = Compose(recipient.Email, recipient.DisplayName, senderName, links.Conversation(conversation.Id), links.TurnOff(job.RecipientAccountId));
        try
        {
            await sender.SendAsync(message, cancellationToken);
            job.MarkSent(clock.UtcNow);
            logger.LogInformation(Sent, "Notification email sent");
        }
        catch (EmailSendException ex)
        {
            job.MarkAttemptFailed(clock.UtcNow);
            if (job.Status == EmailJobStatus.Failed)
            {
                logger.LogWarning(GaveUp, "Notification email not sent after {Attempts} attempts ({ErrorType})", job.Attempts, ex.InnerException?.GetType().Name ?? ex.GetType().Name);
            }
            else
            {
                logger.LogWarning(AttemptFailed, "Notification email attempt {Attempts} failed ({ErrorType})", job.Attempts, ex.InnerException?.GetType().Name ?? ex.GetType().Name);
            }
        }
    }

    /// <summary>BR08: who wrote, a link to the conversation and a link to turn emails off; never the message text.</summary>
    public static EmailMessage Compose(string to, string recipientName, string senderName, string conversationLink, string turnOffLink)
    {
        var hi = FirstName(recipientName);
        var from = FirstName(senderName);
        var subject = $"New message from {from} on Tax Buddy";
        var text = $"Hi {hi},\n\n{from} sent you a message on Tax Buddy. To keep your conversation private, it isn't included here.\n\n" +
                   $"Open the conversation: {conversationLink}\n\n" +
                   $"You're getting this because email notifications are on. Turn them off: {turnOffLink}\n";
        var e = (Func<string, string>)WebUtility.HtmlEncode;
        var html = $"<p>Hi {e(hi)},</p><p>{e(from)} sent you a message on Tax Buddy. To keep your conversation private, it isn't included here.</p>" +
                   $"<p><a href=\"{e(conversationLink)}\">Open the conversation</a></p>" +
                   $"<p style=\"color:#566075;font-size:13px\">You're getting this because email notifications are on. <a href=\"{e(turnOffLink)}\">Turn them off</a>.</p>";
        return new EmailMessage(to, subject, text, html);
    }

    private static string FirstName(string name)
    {
        var first = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        return string.IsNullOrEmpty(first) ? "there" : first;
    }
}

/// <summary>Runs the outbox every few seconds (one server, SDD live updates). Off in tests, which run it directly.</summary>
public sealed class EmailOutboxWorker(IServiceScopeFactory scopes, IOptions<EmailOptions> options, ILogger<EmailOutboxWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.WorkerEnabled)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopes.CreateScope();
                var outbox = scope.ServiceProvider.GetRequiredService<EmailOutbox>();
                while (await outbox.ProcessDueAsync(stoppingToken) == EmailOutbox.BatchSize)
                {
                    // A full batch: there may be more waiting.
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // The database may be down; try again on the next round. Error type only (SDD diagnostics).
                logger.LogWarning(new EventId(1505, "EmailOutboxFailed"), "Email outbox round failed ({ErrorType})", ex.GetType().Name);
            }

            try
            {
                await Task.Delay(options.Value.PollInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}
