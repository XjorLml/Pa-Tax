namespace TaxBuddy.Web.Application.Email;

/// <summary>One email to send (F14). Plain text and HTML with the same words; never the message text (BR08).</summary>
public sealed record EmailMessage(string To, string Subject, string Text, string Html);

/// <summary>
/// Sends notification emails (SDD email contract). Brevo on a site with its key; a folder of files in development; on a
/// site with neither, <see cref="IsConfigured"/> is false and emails are skipped, never queued forever.
/// </summary>
public interface IEmailSender
{
    bool IsConfigured { get; }

    /// <exception cref="EmailSendException">The email service refused or couldn't be reached; the worker retries.</exception>
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken);
}

public sealed class EmailSendException(string message, Exception? inner = null) : Exception(message, inner);
