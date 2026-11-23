using System.Net.Http.Json;
using Microsoft.AspNetCore.DataProtection;
using TaxBuddy.Web.Application.Email;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Services.Email;

/// <summary>
/// Email settings (F14; SDD email contract). In appsettings or the host's secret store:
/// <list type="bullet">
/// <item><c>Email:SiteAddress</c>, the public address that email links point to (development: http://localhost:5234).</item>
/// <item><c>Email:Brevo:ApiKey</c> and <c>Email:SenderAddress</c>: both set means real emails through Brevo (G11).</item>
/// <item><c>Email:SenderName</c> (default "Tax Buddy"); <c>Email:DevelopmentFolder</c>; <c>Email:WorkerEnabled</c>.</item>
/// </list>
/// </summary>
public sealed class EmailOptions
{
    public string SiteAddress { get; set; } = "http://localhost:5234";

    public string SenderName { get; set; } = "Tax Buddy";

    public string? SenderAddress { get; set; }

    public string? BrevoApiKey { get; set; }

    public string? DevelopmentFolder { get; set; }

    public bool WorkerEnabled { get; set; } = true;

    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(15);
}

public static class EmailSetup
{
    public static bool IsBrevoConfigured(IConfiguration configuration) =>
        !string.IsNullOrWhiteSpace(configuration["Email:Brevo:ApiKey"]) && !string.IsNullOrWhiteSpace(configuration["Email:SenderAddress"]);

    public static void AddTaxBuddyEmail(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        var services = builder.Services;
        var defaultFolder = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "..", "artifacts", "emails"));
        services.AddOptions<EmailOptions>().Configure<IConfiguration>((options, config) =>
        {
            options.SiteAddress = (config["Email:SiteAddress"] ?? options.SiteAddress).TrimEnd('/');
            options.SenderName = config["Email:SenderName"] ?? options.SenderName;
            options.SenderAddress = config["Email:SenderAddress"];
            options.BrevoApiKey = config["Email:Brevo:ApiKey"];
            options.DevelopmentFolder = config["Email:DevelopmentFolder"] ?? defaultFolder;
            options.WorkerEnabled = config.GetValue("Email:WorkerEnabled", true);
        });

        if (IsBrevoConfigured(configuration))
        {
            services.AddHttpClient<IEmailSender, BrevoEmailSender>(client =>
            {
                client.BaseAddress = new Uri("https://api.brevo.com/");
                client.Timeout = TimeSpan.FromSeconds(20);
            });
        }
        else if (builder.Environment.IsDevelopment())
        {
            services.AddSingleton<IEmailSender, DevelopmentEmailSender>();
        }
        else
        {
            services.AddSingleton<IEmailSender, NotConfiguredEmailSender>();
        }

        services.AddSingleton<EmailLinks>();
        services.AddScoped<EmailOutbox>();
        services.AddHostedService<EmailOutboxWorker>();
    }
}

/// <summary>SDD development: no real emails. Each email becomes a text file in a folder git ignores (artifacts/emails).</summary>
public sealed class DevelopmentEmailSender(Microsoft.Extensions.Options.IOptions<EmailOptions> options, IClock clock) : IEmailSender
{
    public bool IsConfigured => true;

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        var folder = options.Value.DevelopmentFolder!;
        Directory.CreateDirectory(folder);
        var name = $"{clock.UtcNow:yyyyMMdd-HHmmss-fff}-{Guid.NewGuid():N}"[..28];
        var text = $"To: {message.To}\nSubject: {message.Subject}\n\n{message.Text}";
        await File.WriteAllTextAsync(Path.Combine(folder, name + ".txt"), text, cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(folder, name + ".html"), message.Html, cancellationToken);
    }
}

/// <summary>A deployed site without Brevo: emails are skipped (and logged once per run), not queued forever.</summary>
public sealed class NotConfiguredEmailSender : IEmailSender
{
    public bool IsConfigured => false;

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken) =>
        throw new EmailSendException("Email isn't set up on this site.");
}

/// <summary>Brevo's transactional email API (ADR 007; G11). The key is a server secret and is never logged.</summary>
public sealed class BrevoEmailSender(HttpClient http, Microsoft.Extensions.Options.IOptions<EmailOptions> options) : IEmailSender
{
    public bool IsConfigured => true;

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "v3/smtp/email")
        {
            Content = JsonContent.Create(new
            {
                sender = new { name = settings.SenderName, email = settings.SenderAddress },
                to = new[] { new { email = message.To } },
                subject = message.Subject,
                textContent = message.Text,
                htmlContent = message.Html,
            }),
        };
        request.Headers.Add("api-key", settings.BrevoApiKey);
        request.Headers.Add("accept", "application/json");
        try
        {
            using var response = await http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new EmailSendException($"Brevo answered {(int)response.StatusCode}.");
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new EmailSendException("Brevo couldn't be reached.", ex);
        }
    }
}

/// <summary>
/// Absolute links in emails (F14): the conversation, and a signed link that turns emails off without signing in (R08).
/// The token is the account ID protected by the site's data-protection keys, so it can't be guessed or altered.
/// </summary>
public sealed class EmailLinks(IDataProtectionProvider protection, Microsoft.Extensions.Options.IOptions<EmailOptions> options)
{
    private readonly IDataProtector protector = protection.CreateProtector("TaxBuddy.EmailOff.v1");

    public string Conversation(Guid conversationId) => $"{options.Value.SiteAddress}/chats/{conversationId:D}";

    public string TurnOff(Guid accountId) => $"{options.Value.SiteAddress}{TurnOffPath(accountId)}";

    public string TurnOffPath(Guid accountId) => "/email/off?t=" + Uri.EscapeDataString(Token(accountId));

    public string Token(Guid accountId) => protector.Protect(accountId.ToString("N"));

    public bool TryRead(string? token, out Guid accountId)
    {
        accountId = Guid.Empty;
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        try
        {
            return Guid.TryParseExact(protector.Unprotect(token), "N", out accountId);
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            return false;
        }
    }
}
