using TaxBuddy.Web.Application.Directory;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Application.Profiles;

/// <summary>Where the editor's profiles are kept (implemented in Data).</summary>
public interface IProfileStore
{
    Task<AccountantProfile?> FindByOwnerAsync(Guid ownerAccountId, CancellationToken cancellationToken);

    /// <summary>
    /// Creates or updates the owner's profile from <paramref name="profile"/>, provided its version is still
    /// <paramref name="expectedVersion"/>. Returns the saved profile, or null when another save got there first.
    /// </summary>
    Task<AccountantProfile?> SaveAsync(Guid ownerAccountId, ValidatedProfile profile, int expectedVersion, CancellationToken cancellationToken);
}

/// <summary>Whether a profile is in the directory, and if not, what it still needs (BR01).</summary>
public sealed record ListingStatus(AccountantId Id, bool IsListed, IReadOnlyList<string> Missing);

public abstract record ProfileSaveResult
{
    public sealed record Saved(ListingStatus Status) : ProfileSaveResult;

    public sealed record Invalid(IReadOnlyDictionary<string, string> Errors) : ProfileSaveResult;

    /// <summary>Another tab saved first. <see cref="LatestVersion"/> lets the next save replace it deliberately.</summary>
    public sealed record Conflict(int LatestVersion) : ProfileSaveResult;

    public sealed record Unavailable : ProfileSaveResult;
}

/// <summary>
/// The accountant's own profile (PRD R05; F05). The owner always comes from the signed-in account, never from the form
/// or the address, so nobody can edit another person's profile (BR15).
/// </summary>
public sealed class ProfileEditorService(IProfileStore store)
{
    /// <summary>The form for the owner's profile, or a fresh one with their name when they have none yet.</summary>
    public async Task<(ProfileForm Form, ListingStatus? Status)> LoadAsync(
        Guid ownerAccountId, string? accountName, CancellationToken cancellationToken = default)
    {
        var profile = await store.FindByOwnerAsync(ownerAccountId, cancellationToken);
        return profile is null
            ? (ProfileForm.StartingFrom(accountName), null)
            : (ProfileForm.From(profile), StatusOf(profile));
    }

    public async Task<ProfileSaveResult> SaveAsync(Guid ownerAccountId, ProfileForm form, CancellationToken cancellationToken = default)
    {
        var (validated, errors) = ProfileValidator.Validate(form);
        if (validated is null)
        {
            return new ProfileSaveResult.Invalid(errors);
        }

        try
        {
            var saved = await store.SaveAsync(ownerAccountId, validated, form.Version, cancellationToken);
            if (saved is not null)
            {
                return new ProfileSaveResult.Saved(StatusOf(saved));
            }

            var latest = await store.FindByOwnerAsync(ownerAccountId, cancellationToken);
            return new ProfileSaveResult.Conflict(latest?.Version ?? 0);
        }
        catch (DataUnavailableException)
        {
            return new ProfileSaveResult.Unavailable();
        }
    }

    public static ListingStatus StatusOf(AccountantProfile profile) =>
        new(profile.Id, profile.IsListed, ListingRules.MissingForListing(profile));
}
