using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Data;

/// <summary>
/// Resets the development database's profiles to synthetic data (SETUP: reset-and-seed command; F01 plan).
/// Every person, company and contact detail in the seed file is invented. Runs only in the Development environment.
/// </summary>
public static class DevelopmentSeed
{
    public const string Command = "seed-development";
    private const string ResourceName = "TaxBuddy.Web.Data.Seed.development-profiles.json";

    public static async Task<(int Listed, int Unlisted)> ResetAndSeedAsync(
        IDbContextFactory<TaxBuddyDbContext> contextFactory,
        CancellationToken cancellationToken = default)
    {
        var profiles = LoadProfiles();
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        await db.AccountantProfiles.ExecuteDeleteAsync(cancellationToken);
        db.AccountantProfiles.AddRange(profiles);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var listed = profiles.Count(p => p.IsListed);
        return (listed, profiles.Count - listed);
    }

    public static IReadOnlyList<AccountantProfile> LoadProfiles()
    {
        using var stream = typeof(DevelopmentSeed).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Embedded seed file '{ResourceName}' is missing.");
        var rows = JsonSerializer.Deserialize<List<SeedProfile>>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? throw new InvalidOperationException("The seed file is empty.");

        return rows.Select(r =>
        {
            var profile = new AccountantProfile(
                new AccountantId(r.Id), r.DisplayName, r.Title, r.City, r.Summary, r.Email, r.Phone, r.Website, r.ConsentToShowContact);
            foreach (var service in r.Services)
            {
                profile.AddService(service.Name, service.Description);
            }

            foreach (var entry in r.Experience)
            {
                profile.AddExperience(
                    entry.Role, entry.Organization, YearMonth.Parse(entry.Start), entry.End is null ? null : YearMonth.Parse(entry.End));
            }

            return profile;
        }).ToList();
    }

    private sealed record SeedProfile(
        Guid Id,
        string DisplayName,
        string Title,
        string? City,
        string Summary,
        string Email,
        string? Phone,
        string? Website,
        bool ConsentToShowContact,
        List<SeedService> Services,
        List<SeedExperience> Experience);

    private sealed record SeedService(string Name, string Description);

    private sealed record SeedExperience(string Role, string Organization, string Start, string? End);
}
