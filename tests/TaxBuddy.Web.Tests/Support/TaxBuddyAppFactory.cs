using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using TaxBuddy.TestSupport;
using TaxBuddy.Web.Data;

namespace TaxBuddy.Web.Tests.Support;

/// <summary>Hosts the app in memory against a given database (TEST-PLAN: web integration tests).</summary>
public sealed class TaxBuddyAppFactory(string connectionString) : WebApplicationFactory<Program>
{
    /// <summary>An address where nothing listens, so every database call fails at once (FT06, TP08).</summary>
    public const string UnreachableDatabase = "Host=127.0.0.1;Port=1;Database=none;Username=none;Password=none;Timeout=3";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Not Development: no developer exception page and no user-secrets; the test supplies the connection string.
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:TaxBuddy"] = connectionString }));
    }

    public HttpClient CreateClientWithoutRedirects() =>
        CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
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
