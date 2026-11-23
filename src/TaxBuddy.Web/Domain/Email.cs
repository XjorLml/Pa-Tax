namespace TaxBuddy.Web.Domain;

/// <summary>Where a queued notification email stands (F14, PRD R08).</summary>
public enum EmailJobStatus
{
    Pending,
    Sent,

    /// <summary>Not sent, on purpose: the recipient read the conversation first, turned emails off, or left.</summary>
    Skipped,

    /// <summary>Not sent after <see cref="EmailRules.MaxAttempts"/> tries. The message itself is unaffected.</summary>
    Failed,
}

/// <summary>
/// One notification email about new messages in one conversation, written in the same transaction as the message
/// (SDD email contract). A background worker sends it once it is due; it never holds the message text (BR08).
/// </summary>
public sealed class EmailJob
{
    private EmailJob()
    {
    }

    public EmailJob(Guid id, Guid recipientAccountId, Guid conversationId, DateTimeOffset createdAt)
    {
        Id = id;
        RecipientAccountId = recipientAccountId;
        ConversationId = conversationId;
        CreatedAt = createdAt;
        DueAt = createdAt + EmailRules.Delay;
        Status = EmailJobStatus.Pending;
    }

    public Guid Id { get; private set; }

    public Guid RecipientAccountId { get; private set; }

    public Guid ConversationId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>When the worker may next try: the delay after the message, then each retry's wait.</summary>
    public DateTimeOffset DueAt { get; private set; }

    public EmailJobStatus Status { get; private set; }

    public int Attempts { get; private set; }

    public DateTimeOffset? FinishedAt { get; private set; }

    public void MarkSent(DateTimeOffset at)
    {
        Attempts++;
        Status = EmailJobStatus.Sent;
        FinishedAt = at;
    }

    public void MarkSkipped(DateTimeOffset at)
    {
        Status = EmailJobStatus.Skipped;
        FinishedAt = at;
    }

    /// <summary>A failed try: wait longer each time, then give up.</summary>
    public void MarkAttemptFailed(DateTimeOffset at)
    {
        Attempts++;
        if (Attempts >= EmailRules.MaxAttempts)
        {
            Status = EmailJobStatus.Failed;
            FinishedAt = at;
        }
        else
        {
            DueAt = at + EmailRules.RetryAfter(Attempts);
        }
    }
}

/// <summary>Timing rules for notification emails (F14; PRD D18, decided 2026-11-23).</summary>
public static class EmailRules
{
    /// <summary>
    /// How long a message waits before its email goes. If the recipient reads the conversation in that time (for
    /// example, they have it open), no email is sent: the site's stand-in for "when they are not on the site" (R08).
    /// </summary>
    public static readonly TimeSpan Delay = TimeSpan.FromMinutes(2);

    public const int MaxAttempts = 6;

    /// <summary>How long sent, skipped and failed jobs are kept before the worker removes them.</summary>
    public static readonly TimeSpan KeepFinished = TimeSpan.FromDays(30);

    public static TimeSpan RetryAfter(int attempts) => attempts switch
    {
        1 => TimeSpan.FromMinutes(1),
        2 => TimeSpan.FromMinutes(5),
        3 => TimeSpan.FromMinutes(30),
        _ => TimeSpan.FromHours(2),
    };

    /// <summary>
    /// D18: at most one email per conversation until the recipient opens it. A new job is due only when none was
    /// created after the recipient last read the conversation.
    /// </summary>
    public static bool NeedsNewJob(DateTimeOffset? recipientReadAt, DateTimeOffset? latestJobCreatedAt) =>
        latestJobCreatedAt is null || (recipientReadAt is not null && recipientReadAt >= latestJobCreatedAt);
}
