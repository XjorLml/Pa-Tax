using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Data;

/// <summary>
/// The application's database (SDD: data and persistence contracts). Interactive pages and services take a new
/// context from <see cref="IDbContextFactory{TContext}"/> for each operation.
/// </summary>
public sealed partial class TaxBuddyDbContext(DbContextOptions<TaxBuddyDbContext> options) : DbContext(options)
{
    public const int ServiceNameLength = 40;

    public DbSet<AccountantProfile> AccountantProfiles => Set<AccountantProfile>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<AccountantId>().HaveConversion<AccountantIdConverter>();
        configurationBuilder.Properties<YearMonth>().HaveConversion<YearMonthConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountantProfile>(profile =>
        {
            profile.ToTable("accountant_profiles");
            profile.HasKey(p => p.Id);
            profile.Property(p => p.Id).ValueGeneratedNever();
            profile.Property(p => p.DisplayName).HasMaxLength(ProfileLimits.DisplayName);
            profile.Property(p => p.Title).HasMaxLength(ProfileLimits.Title);
            profile.Property(p => p.City).HasMaxLength(ProfileLimits.City);
            profile.Property(p => p.Summary).HasMaxLength(ProfileLimits.Summary);
            profile.Property(p => p.Email).HasMaxLength(ProfileLimits.Email);
            profile.Property(p => p.Phone).HasMaxLength(ProfileLimits.Phone);
            profile.Property(p => p.Website).HasMaxLength(ProfileLimits.Website);
            profile.Ignore(p => p.IsListed);

            profile.OwnsMany(p => p.Services, service =>
            {
                service.ToTable("accountant_services");
                service.WithOwner().HasForeignKey("AccountantProfileId");
                service.Property(s => s.Name).HasMaxLength(ServiceNameLength);
                service.Property(s => s.Description).HasMaxLength(ProfileLimits.ServiceDescription);
                service.HasIndex("AccountantProfileId", nameof(OfferedService.Name)).IsUnique();
            });

            profile.OwnsMany(p => p.Experience, entry =>
            {
                entry.ToTable("accountant_experience");
                entry.WithOwner().HasForeignKey("AccountantProfileId");
                entry.Property(e => e.Role).HasMaxLength(ProfileLimits.ExperienceText);
                entry.Property(e => e.Organization).HasMaxLength(ProfileLimits.ExperienceText);
                entry.Property(e => e.Start).HasColumnName("start_month");
                entry.Property(e => e.End).HasColumnName("end_month");
                entry.Ignore(e => e.IsCurrent);
            });
        });

        ApplySnakeCaseNames(modelBuilder);
    }

    /// <summary>PostgreSQL convention: snake_case names, so plain SQL needs no quoting.</summary>
    private static void ApplySnakeCaseNames(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties())
            {
                if (property.GetColumnName() is { } column)
                {
                    property.SetColumnName(ToSnakeCase(column));
                }
            }

            foreach (var key in entity.GetKeys())
            {
                if (key.GetName() is { } name)
                {
                    key.SetName(ToSnakeCase(name));
                }
            }

            // fk_<table>_<columns>: unique per table and short enough for PostgreSQL's 63-character limit.
            foreach (var foreignKey in entity.GetForeignKeys())
            {
                var columns = string.Join("_", foreignKey.Properties.Select(p => ToSnakeCase(p.GetColumnName())));
                foreignKey.SetConstraintName($"fk_{entity.GetTableName()}_{columns}");
            }

            foreach (var index in entity.GetIndexes())
            {
                if (index.GetDatabaseName() is { } name)
                {
                    index.SetDatabaseName(ToSnakeCase(name));
                }
            }
        }
    }

    private static string ToSnakeCase(string name) => UpperAfterLower().Replace(name, "_$1").ToLowerInvariant();

    [GeneratedRegex("(?<=[a-z0-9])([A-Z])")]
    private static partial Regex UpperAfterLower();
}

public sealed class AccountantIdConverter() : ValueConverter<AccountantId, Guid>(id => id.Value, value => new AccountantId(value));

/// <summary>Stores a <see cref="YearMonth"/> as the first day of the month in a <c>date</c> column, so it sorts naturally.</summary>
public sealed class YearMonthConverter() : ValueConverter<YearMonth, DateOnly>(month => month.FirstDay, date => YearMonth.From(date));
