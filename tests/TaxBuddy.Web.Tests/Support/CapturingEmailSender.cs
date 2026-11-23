using System.Collections.Concurrent;
using TaxBuddy.Web.Application.Email;

namespace TaxBuddy.Web.Tests.Support;

/// <summary>Stands in for Brevo in web tests (F14). Keeps every email; can be told to fail, to check retries.</summary>
public sealed class CapturingEmailSender : IEmailSender
{
    public ConcurrentQueue<EmailMessage> Sent { get; } = new();

    public bool Fail { get; set; }

    public bool IsConfigured => true;

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        if (Fail)
        {
            throw new EmailSendException("Simulated email failure.");
        }

        Sent.Enqueue(message);
        return Task.CompletedTask;
    }
}
