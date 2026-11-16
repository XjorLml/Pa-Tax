using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using TaxBuddy.Web.Application.Directory;
using TaxBuddy.Web.Application.Profiles;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Data;

/// <summary>The accountant's own profile on PostgreSQL (F05; SDD: identity and mutation, optimistic concurrency).</summary>
public sealed class ProfileStore(IDbContextFactory<TaxBuddyDbContext> contextFactory, ILogger<ProfileStore> logger) : IProfileStore
{
    private static readonly EventId DataUnavailable = new(1002, "ProfileDataUnavailable");

    public async Task<AccountantProfile?> FindByOwnerAsync(Guid ownerAccountId, CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
            return await db.AccountantProfiles.AsNoTracking()
                .FirstOrDefaultAsync(p => p.OwnerAccountId == ownerAccountId, cancellationToken);
        }
        catch (Exception ex) when (IsDataFailure(ex))
        {
            throw Unavailable(ex);
        }
    }

    public async Task<AccountantProfile?> SaveAsync(
        Guid ownerAccountId, ValidatedProfile values, int expectedVersion, CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
            var profile = await db.AccountantProfiles.FirstOrDefaultAsync(p => p.OwnerAccountId == ownerAccountId, cancellationToken);
            if (profile is null)
            {
                if (expectedVersion != 0)
                {
                    return null; // The form came from a profile that no longer exists.
                }

                profile = AccountantProfile.CreateFor(ownerAccountId, values.DisplayName, values.Email);
                db.AccountantProfiles.Add(profile);
            }
            else
            {
                // The database refuses the update if another save changed the version since the form was loaded.
                db.Entry(profile).Property(p => p.Version).OriginalValue = expectedVersion;
            }

            profile.Edit(values.DisplayName, values.Title, values.City, values.Summary, values.Email, values.Phone, values.Website,
                values.ConsentToShowContact);
            profile.ReplaceServices(values.Services);
            profile.ReplaceExperience(values.Experience);
            profile.MarkSaved();

            await db.SaveChangesAsync(cancellationToken);
            return profile;
        }
        catch (DbUpdateConcurrencyException)
        {
            return null;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return null; // Two first saves at once: the other one created the profile.
        }
        catch (Exception ex) when (IsDataFailure(ex))
        {
            throw Unavailable(ex);
        }
    }

    private static bool IsDataFailure(Exception ex) => ex switch
    {
        DbException or TimeoutException or RetryLimitExceededException or DbUpdateException => true,
        InvalidOperationException { InnerException: DbException or TimeoutException } => true,
        _ => false,
    };

    private DataUnavailableException Unavailable(Exception ex)
    {
        // SDD diagnostics: event ID and error category only; never profile text or contact details.
        logger.LogWarning(DataUnavailable, "Profile data unavailable ({ErrorType})", ex.GetType().Name);
        return new DataUnavailableException(ex);
    }
}
