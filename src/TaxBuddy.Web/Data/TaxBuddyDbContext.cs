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

    public DbSet<Review> Reviews => Set<Review>();

    public DbSet<Report> Reports => Set<Report>();

    public DbSet<EmailJob> EmailJobs => Set<EmailJob>();

    public DbSet<SavedAccountant> SavedAccountants => Set<SavedAccountant>();

    public DbSet<ServiceEngagement> ServiceEngagements => Set<ServiceEngagement>();

    public DbSet<ServiceTask> ServiceTasks => Set<ServiceTask>();

    public DbSet<ServiceUpdate> ServiceUpdates => Set<ServiceUpdate>();

    public DbSet<ServiceFile> ServiceFiles => Set<ServiceFile>();

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
        ConfigureReviews(modelBuilder);
        ConfigureReports(modelBuilder);
        ConfigureEmailJobs(modelBuilder);
        ConfigureWork(modelBuilder);
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

    /// <summary>
    /// F09: one review per client and accountant (BR09), deleted with the profile. As with chat, deleting the client's
    /// account is refused until D07 decides what happens to their reviews.
    /// </summary>
    private static void ConfigureReviews(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Review>(review =>
        {
            review.ToTable("reviews", table => table.HasCheckConstraint(
                "ck_reviews_stars", $"stars BETWEEN {ReviewLimits.MinStars} AND {ReviewLimits.MaxStars}"));
            review.HasKey(r => r.Id);
            review.Property(r => r.Id).ValueGeneratedNever();
            review.Property(r => r.Text).HasMaxLength(ReviewLimits.Text);
            review.Property(r => r.ReplyText).HasMaxLength(ReviewLimits.Reply);
            review.HasIndex(r => new { r.AccountantId, r.ClientAccountId }).IsUnique();
            review.HasIndex(r => r.ClientAccountId);
            review.HasOne<AccountantProfile>().WithMany().HasForeignKey(r => r.AccountantId).OnDelete(DeleteBehavior.Cascade);
            review.HasOne<TaxBuddyUser>().WithMany().HasForeignKey(r => r.ClientAccountId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    /// <summary>
    /// F10: reports to the site owner, read in the database (D22). One per reporter and item. No foreign key to the item:
    /// a report outlives the content it is about. Deleting the reporter's account is refused until D07, as for chat.
    /// </summary>
    private static void ConfigureReports(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Report>(report =>
        {
            report.ToTable("reports");
            report.HasKey(r => r.Id);
            report.Property(r => r.Id).ValueGeneratedNever();
            report.Property(r => r.Kind).HasConversion<string>().HasMaxLength(20);
            report.Property(r => r.Reason).HasConversion<string>().HasMaxLength(40);
            report.Property(r => r.Details).HasMaxLength(ReportLimits.Details);
            report.HasIndex(r => new { r.ReporterAccountId, r.Kind, r.TargetId }).IsUnique();
            report.HasIndex(r => new { r.ReporterAccountId, r.CreatedAt });
            report.HasIndex(r => r.CreatedAt);
            report.HasOne<TaxBuddyUser>().WithMany().HasForeignKey(r => r.ReporterAccountId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    /// <summary>
    /// F14: notification emails waiting to be sent (SDD email contract). They go with their conversation; the recipient's
    /// account row is never deleted (F12), so its link refuses deletion as the other tables do.
    /// </summary>
    private static void ConfigureEmailJobs(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EmailJob>(job =>
        {
            job.ToTable("email_jobs");
            job.HasKey(j => j.Id);
            job.Property(j => j.Id).ValueGeneratedNever();
            job.Property(j => j.Status).HasConversion<string>().HasMaxLength(20);
            job.HasIndex(j => new { j.Status, j.DueAt });
            job.HasIndex(j => new { j.RecipientAccountId, j.ConversationId, j.CreatedAt });
            job.HasOne<Conversation>().WithMany().HasForeignKey(j => j.ConversationId).OnDelete(DeleteBehavior.Cascade);
            job.HasOne<TaxBuddyUser>().WithMany().HasForeignKey(j => j.RecipientAccountId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    /// <summary>
    /// F03 service work (SDD private service workspaces). A saved entry goes with its profile. A service, its tasks and
    /// updates stay with their two accounts, whose rows are never deleted (F12). One active or requested service per
    /// client, accountant and service name (D24 proposal: no duplicates); declined ones don't count.
    /// </summary>
    private static void ConfigureWork(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SavedAccountant>(saved =>
        {
            saved.ToTable("saved_accountants");
            saved.HasKey(s => new { s.ClientAccountId, s.ProfileId });
            saved.HasOne<TaxBuddyUser>().WithMany().HasForeignKey(s => s.ClientAccountId).OnDelete(DeleteBehavior.Restrict);
            saved.HasOne<AccountantProfile>().WithMany().HasForeignKey(s => s.ProfileId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ServiceEngagement>(engagement =>
        {
            engagement.ToTable("service_engagements");
            engagement.HasKey(e => e.Id);
            engagement.Property(e => e.Id).ValueGeneratedNever();
            engagement.Property(e => e.ServiceName).HasMaxLength(ServiceNameLength);
            engagement.Property(e => e.Phase).HasConversion<string>().HasMaxLength(20);
            engagement.Property(e => e.Version).IsConcurrencyToken();
            engagement.HasIndex(e => new { e.ClientAccountId, e.AccountantAccountId, e.ServiceName })
                .IsUnique().HasFilter("phase <> 'Declined'");
            engagement.HasIndex(e => e.AccountantAccountId);
            engagement.HasOne<TaxBuddyUser>().WithMany().HasForeignKey(e => e.ClientAccountId).OnDelete(DeleteBehavior.Restrict);
            engagement.HasOne<TaxBuddyUser>().WithMany().HasForeignKey(e => e.AccountantAccountId).OnDelete(DeleteBehavior.Restrict);
            engagement.HasMany(e => e.Tasks).WithOne().HasForeignKey(t => t.EngagementId).OnDelete(DeleteBehavior.Cascade);
            engagement.Navigation(e => e.Tasks).HasField("tasks").UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<ServiceTask>(task =>
        {
            task.ToTable("service_tasks");
            task.HasKey(t => t.Id);
            task.Property(t => t.Id).ValueGeneratedNever();
            task.Property(t => t.Title).HasMaxLength(WorkRules.TaskTitle);
            task.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<ServiceUpdate>(update =>
        {
            update.ToTable("service_updates");
            update.HasKey(u => u.Id);
            update.Property(u => u.Id).ValueGeneratedNever();
            update.Property(u => u.Text).HasMaxLength(WorkRules.Note);
            update.HasIndex(u => new { u.EngagementId, u.CreatedAt });
            update.HasOne<ServiceEngagement>().WithMany().HasForeignKey(u => u.EngagementId).OnDelete(DeleteBehavior.Cascade);
            update.HasOne<TaxBuddyUser>().WithMany().HasForeignKey(u => u.AuthorAccountId).OnDelete(DeleteBehavior.Restrict);
        });

        // F03 stage 2 (R23): documents go with their service; a task link is cleared if the task ever goes. The uploader's
        // account row is never deleted (F12): their documents are deleted with the account instead (D27, partly).
        modelBuilder.Entity<ServiceFile>(file =>
        {
            file.ToTable("service_files", table => table.HasCheckConstraint(
                "ck_service_files_length", $"length BETWEEN 1 AND {DocumentRules.MaxBytes}"));
            file.HasKey(f => f.Id);
            file.Property(f => f.Id).ValueGeneratedNever();
            file.Property(f => f.ObjectName).HasMaxLength(200);
            file.Property(f => f.DisplayName).HasMaxLength(DocumentRules.DisplayName);
            file.Property(f => f.ContentType).HasMaxLength(40);
            file.HasIndex(f => f.ObjectName).IsUnique();
            file.HasIndex(f => new { f.UploaderAccountId, f.OperationId }).IsUnique();
            file.HasIndex(f => new { f.EngagementId, f.CreatedAt });
            file.HasOne<ServiceEngagement>().WithMany().HasForeignKey(f => f.EngagementId).OnDelete(DeleteBehavior.Cascade);
            file.HasOne<ServiceTask>().WithMany().HasForeignKey(f => f.TaskId).OnDelete(DeleteBehavior.SetNull);
            file.HasOne<TaxBuddyUser>().WithMany().HasForeignKey(f => f.UploaderAccountId).OnDelete(DeleteBehavior.Restrict);
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
            // F17 (PRD R25): a client's details.
            user.Property(u => u.PhoneNumber).HasMaxLength(ClientDetailsRules.PhoneMax);
            user.Property(u => u.City).HasMaxLength(ClientDetailsRules.City);
            user.Property(u => u.BusinessName).HasMaxLength(ClientDetailsRules.BusinessName);
            user.Property(u => u.BusinessType).HasMaxLength(ClientDetailsRules.BusinessType);
            user.Property(u => u.Industry).HasMaxLength(ClientDetailsRules.Industry);
            user.Ignore(u => u.ClientDetails);
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
