using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using TaxBuddy.Web.Data;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.TestSupport;

/// <summary>
/// Data tests run against the real PostgreSQL provider (DEVELOPMENT, TEST-PLAN), each test class in its own throwaway
/// schema inside the development database (SETUP: data tests in their own schema). The schema is dropped afterwards;
/// schemas left by a crashed run are dropped after six hours. Without a connection string the tests are skipped.
/// </summary>
public static class TestDatabase
{
    public const string SkipReason =
        "No test database: set ConnectionStrings:TaxBuddy in user-secrets 'taxbuddy-web' or the ConnectionStrings__TaxBuddy environment variable (SETUP).";

    private const string Prefix = "test_";

    private static readonly Lazy<string?> BaseConnectionString = new(() =>
        new ConfigurationBuilder()
            .AddUserSecrets("taxbuddy-web")
            .AddEnvironmentVariables()
            .Build()
            .GetConnectionString("TaxBuddy"));

    public static bool IsConfigured => !string.IsNullOrWhiteSpace(BaseConnectionString.Value);

    public static async Task<TestSchema> CreateSchemaAsync()
    {
        var baseConnection = BaseConnectionString.Value ?? throw new InvalidOperationException(SkipReason);
        var name = $"{Prefix}{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}"[..36];

        await using (var admin = new NpgsqlConnection(baseConnection))
        {
            await admin.OpenAsync();
            await DropStaleSchemasAsync(admin);
            await using var create = new NpgsqlCommand($"CREATE SCHEMA \"{name}\"", admin);
            await create.ExecuteNonQueryAsync();
        }

        var connectionString = new NpgsqlConnectionStringBuilder(baseConnection)
        {
            SearchPath = name,
            MaxPoolSize = 5,
        }.ConnectionString;

        var schema = new TestSchema(name, connectionString, baseConnection);
        await using var db = schema.CreateContext();
        await db.Database.MigrateAsync();
        return schema;
    }

    private static async Task DropStaleSchemasAsync(NpgsqlConnection admin)
    {
        var stale = new List<string>();
        await using (var list = new NpgsqlCommand("SELECT nspname FROM pg_namespace WHERE nspname LIKE 'test\\_%'", admin))
        await using (var reader = await list.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                var schema = reader.GetString(0);
                if (schema.Length >= Prefix.Length + 14
                    && DateTime.TryParseExact(schema.Substring(Prefix.Length, 14), "yyyyMMddHHmmss", CultureInfo.InvariantCulture,
                        DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var created)
                    && created < DateTime.UtcNow.AddHours(-6))
                {
                    stale.Add(schema);
                }
            }
        }

        foreach (var schema in stale)
        {
            await using var drop = new NpgsqlCommand($"DROP SCHEMA IF EXISTS \"{schema}\" CASCADE", admin);
            await drop.ExecuteNonQueryAsync();
        }
    }
}

/// <summary>A migrated, isolated schema. Dispose drops it.</summary>
public sealed class TestSchema(string name, string connectionString, string baseConnectionString)
    : IDbContextFactory<TaxBuddyDbContext>, IAsyncDisposable
{
    public string Name => name;

    /// <summary>A connection string whose search path is this schema, for test hosts.</summary>
    public string ConnectionString => connectionString;

    public TaxBuddyDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<TaxBuddyDbContext>().UseNpgsql(connectionString).Options);

    TaxBuddyDbContext IDbContextFactory<TaxBuddyDbContext>.CreateDbContext() => CreateContext();

    public async Task AddAsync(params AccountantProfile[] profiles)
    {
        await using var db = CreateContext();
        db.AccountantProfiles.AddRange(profiles);
        await db.SaveChangesAsync();
    }

    public async Task ClearAsync()
    {
        await using var db = CreateContext();
        await db.AccountantProfiles.ExecuteDeleteAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await using (var pooled = new NpgsqlConnection(connectionString))
        {
            NpgsqlConnection.ClearPool(pooled);
        }

        await using var admin = new NpgsqlConnection(baseConnectionString);
        await admin.OpenAsync();
        await using var drop = new NpgsqlCommand($"DROP SCHEMA IF EXISTS \"{name}\" CASCADE", admin);
        await drop.ExecuteNonQueryAsync();
    }
}

/// <summary>A fact that is skipped when no test database is configured.</summary>
public sealed class DatabaseFactAttribute : FactAttribute
{
    public DatabaseFactAttribute()
    {
        if (!TestDatabase.IsConfigured)
        {
            Skip = TestDatabase.SkipReason;
        }
    }
}

/// <summary>A theory that is skipped when no test database is configured.</summary>
public sealed class DatabaseTheoryAttribute : TheoryAttribute
{
    public DatabaseTheoryAttribute()
    {
        if (!TestDatabase.IsConfigured)
        {
            Skip = TestDatabase.SkipReason;
        }
    }
}

/// <summary>Creates one schema for a test class; does nothing when no database is configured.</summary>
public class SchemaFixture : IAsyncLifetime
{
    private TestSchema? schema;

    public TestSchema Schema => schema ?? throw new InvalidOperationException(TestDatabase.SkipReason);

    public virtual async Task InitializeAsync()
    {
        if (TestDatabase.IsConfigured)
        {
            schema = await TestDatabase.CreateSchemaAsync();
        }
    }

    public async Task DisposeAsync()
    {
        if (schema is not null)
        {
            await schema.DisposeAsync();
        }
    }
}
