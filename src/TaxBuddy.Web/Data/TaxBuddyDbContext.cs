using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Data;

/// <summary>
/// The application's database (SDD: data and persistence contracts). Interactive pages and services take a new
/// context from <see cref="IDbContextFactory{TContext}"/> for each operation. It is also Identity's user store (ADR 006):
/// users, their external sign-in links, claims and tokens. There are no Identity role tables, because an account's one
/// role is a column (PRD D03).
/// </summary>
public sealed partial class TaxBuddyDbContext(DbContextOptions<TaxBuddyDbContext> options)
    : IdentityUserContext<TaxBuddyUser, Guid>(options)
{
    public const int ServiceNameLength = 40;

    public DbSet<AccountantProfile> AccountantProfiles => Set<AccountantProfile>();

    public DbSet<Conversation> Conversations => Set<Conversation>();

    public DbSet<ChatMessage> Messages => Set<ChatMessage>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<AccountantId>().HaveConversion<AccountantIdConverter>();
        configurationBuilder.Properties<YearMonth>().HaveConversion<YearMonthConverter>();
        configurationBuilder.Properties<AccountRole>().HaveConversion<string>().HaveMaxLength(20);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ConfigureAccounts(modelBuilder);

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

            // F05: one profile per accountant account, deleted with the account (BR06). Seed profiles have no owner.
            profile.HasIndex(p => p.OwnerAccountId).IsUnique();
            profile.HasOne<TaxBuddyUser>().WithOne().HasForeignKey<AccountantProfile>(p => p.OwnerAccountId)
                .OnDelete(DeleteBehavior.Cascade);
            profile.Property(p => p.Version).IsConcurrencyToken();

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

            // F08: portfolio highlights, deleted with their profile.
            profile.OwnsMany(p => p.Highlights, highlight =>
            {
                highlight.ToTable("accountant_highlights");
                highlight.WithOwner().HasForeignKey("AccountantProfileId");
                highlight.Property(h => h.Title).HasMaxLength(ProfileLimits.HighlightTitle);
                highlight.Property(h => h.ClientType).HasMaxLength(ProfileLimits.HighlightClientType);
                highlight.Property(h => h.Work).HasMaxLength(ProfileLimits.HighlightWork);
                highlight.Property(h => h.Result).HasMaxLength(ProfileLimits.HighlightResult);
            });
        });

        ConfigureChat(modelBuilder);
        ApplySnakeCaseNames(modelBuilder);
    }

    /// <summary>
    /// F07: one conversation per client and accountant (D06). Deleting an account does not remove the other person's copy
    /// here: what happens then is open (D07), so the database refuses it until account deletion (R09) decides.
    /// </summary>
    private static void ConfigureChat(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Conversation>(conversation =>
        {
            conversation.ToTable("conversations");
            conversation.HasKey(c => c.Id);
            conversation.Property(c => c.Id).ValueGeneratedNever();
            conversation.HasIndex(c => new { c.ClientAccountId, c.AccountantAccountId }).IsUnique();
            conversation.HasIndex(c => c.AccountantAccountId);
            conversation.HasOne<TaxBuddyUser>().WithMany().HasForeignKey(c => c.ClientAccountId).OnDelete(DeleteBehavior.Restrict);
            conversation.HasOne<TaxBuddyUser>().WithMany().HasForeignKey(c => c.AccountantAccountId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ChatMessage>(message =>
        {
            message.ToTable("messages");
            message.HasKey(m => m.Id);
            message.Property(m => m.Id).ValueGeneratedNever();
            message.Property(m => m.Text).HasMaxLength(ChatLimits.MessageLength);
            message.HasIndex(m => new { m.ConversationId, m.SentAt });
            message.HasIndex(m => new { m.SenderAccountId, m.SentAt });
            message.HasOne<Conversation>().WithMany().HasForeignKey(m => m.ConversationId).OnDelete(DeleteBehavior.Cascade);
            message.HasOne<TaxBuddyUser>().WithMany().HasForeignKey(m => m.SenderAccountId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    /// <summary>F04: Identity's tables under plain names, and the account fields.</summary>
    private static void ConfigureAccounts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaxBuddyUser>(user =>
        {
            user.ToTable("users");
            user.Property(u => u.DisplayName).HasMaxLength(AccountLimits.DisplayName);
            user.Property(u => u.Email).HasMaxLength(AccountLimits.Email);
            user.Property(u => u.NormalizedEmail).HasMaxLength(AccountLimits.Email);
            user.Property(u => u.AgreedTermsVersion).HasMaxLength(AccountLimits.TermsVersion);
            user.Property(u => u.PictureObject).HasMaxLength(200);
        });

        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("user_logins");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("user_claims");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens");
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
