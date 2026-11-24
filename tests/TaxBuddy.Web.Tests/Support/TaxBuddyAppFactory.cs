using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaxBuddy.TestSupport;
using TaxBuddy.Web.Data;

namespace TaxBuddy.Web.Tests.Support;

/// <summary>
/// Hosts the app in memory against a given database (TEST-PLAN: web integration tests). Google sign-in is configured
/// with dummy values unless <paramref name="withGoogle"/> is false. Archived Microsoft sign-in (F15) gets dummy keys only
/// when <paramref name="withMicrosoft"/> is true, and is switched on with them unless <paramref name="microsoftEnabled"/>
/// says otherwise. The test sign-in stands in for the provider's round trip.
/// </summary>
public sealed class TaxBuddyAppFactory(
    string connectionString, bool withGoogle = true, bool withPictureStorage = true, bool withMicrosoft = false, bool withDocumentStorage = true,
    bool? microsoftEnabled = null, IReadOnlyDictionary<string, string>? settings = null)
    : WebApplicationFactory<Program>
{
    public const string GoogleClientId = "test-client-id.apps.googleusercontent.com";

    public const string MicrosoftClientId = "00000000-0000-4000-8000-000000000015";

    public CapturedLogs Logs { get; } = new();

    /// <summary>Replaces Supabase Storage (F06) unless the factory was created without picture storage.</summary>
    public InMemoryPictureStorage Pictures { get; } = new();

    /// <summary>Replaces the service documents bucket (F03 stage 2) unless the factory was created without it.</summary>
    public InMemoryDocumentStorage Documents { get; } = new();

    /// <summary>Replaces the email service (F14): emails are captured, and can be told to fail.</summary>
    public CapturingEmailSender Emails { get; } = new();

    /// <summary>An address where nothing listens, so every database call fails at once (FT06, TP08).</summary>
    public const string UnreachableDatabase = "Host=127.0.0.1;Port=1;Database=none;Username=none;Password=none;Timeout=3";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Not Development: no developer exception page and no user-secrets; the test supplies the connection string.
        builder.UseEnvironment("Testing");

        // Read while the app is being built (AccountSetup decides whether to register Google), so these are host
        // settings rather than the app configuration below, which arrives later.
        builder.UseSetting("Authentication:Google:ClientId", withGoogle ? GoogleClientId : "");
        builder.UseSetting("Authentication:Google:ClientSecret", withGoogle ? "test-client-secret" : "");
        builder.UseSetting("Authentication:Microsoft:ClientId", withMicrosoft ? MicrosoftClientId : "");
        builder.UseSetting("Authentication:Microsoft:ClientSecret", withMicrosoft ? "test-microsoft-secret" : "");
        builder.UseSetting(TaxBuddy.Web.Account.AccountSetup.MicrosoftEnabledSetting, (microsoftEnabled ?? withMicrosoft) ? "true" : "false");
        // F14: tests run the email outbox themselves, so its background worker stays off.
        builder.UseSetting("Email:WorkerEnabled", "false");
        builder.UseSetting("Email:SiteAddress", "https://taxbuddy.test");
        foreach (var (key, value) in settings ?? new Dictionary<string, string>())
        {
            builder.UseSetting(key, value); // e.g. the container's settings (ADR 012)
        }
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<Microsoft.AspNetCore.Hosting.IStartupFilter, TestExternalSignIn>();
            services.AddSingleton<ILoggerProvider>(Logs);
            services.AddSingleton<TaxBuddy.Web.Application.Email.IEmailSender>(Emails);
            if (withPictureStorage)
            {
                services.AddSingleton<TaxBuddy.Web.Application.Pictures.IPictureStorage>(Pictures);
            }

            if (withDocumentStorage)
            {
                services.AddSingleton<TaxBuddy.Web.Application.Work.IDocumentStorage>(Documents);
            }
        });
        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:TaxBuddy"] = connectionString }));
    }

    public HttpClient CreateClientWithoutRedirects() =>
        CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    /// <summary>
    /// Over HTTPS, as a browser on a deployed site: outside development the session cookies are Secure (SDD), so a
    /// plain-HTTP client would never send them back. Redirects are not followed.
    /// </summary>
    public HttpClient CreateHttpsClient() =>
        CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, BaseAddress = new Uri("https://localhost") });
}

/// <summary>A schema holding the development seed: 8 listed synthetic accountants and 1 without consent.</summary>
public sealed class SeededAppFixture : SchemaFixture, IAsyncLifetime
{
    private TaxBuddyAppFactory? factory;

    public TaxBuddyAppFactory Factory => factory ?? throw new InvalidOperationException(TestDatabase.SkipReason);

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        if (TestDatabase.IsConfigured)
        {
            await DevelopmentSeed.ResetAndSeedAsync(Schema);
            factory = new TaxBuddyAppFactory(Schema.ConnectionString);
        }
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        if (factory is not null)
        {
            await factory.DisposeAsync();
        }

        await DisposeAsync();
    }
}

/// <summary>A migrated schema with no profiles.</summary>
public sealed class EmptyAppFixture : SchemaFixture, IAsyncLifetime
{
    private TaxBuddyAppFactory? factory;

    public TaxBuddyAppFactory Factory => factory ?? throw new InvalidOperationException(TestDatabase.SkipReason);

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        if (TestDatabase.IsConfigured)
        {
            factory = new TaxBuddyAppFactory(Schema.ConnectionString);
        }
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        if (factory is not null)
        {
            await factory.DisposeAsync();
        }

        await DisposeAsync();
    }
}
