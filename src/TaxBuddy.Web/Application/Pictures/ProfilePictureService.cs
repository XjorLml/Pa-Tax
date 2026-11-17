using TaxBuddy.Web.Application.Directory;

namespace TaxBuddy.Web.Application.Pictures;

/// <summary>Where picture bytes are kept: a private Supabase Storage bucket (ADR 011). Implemented in Data.</summary>
public interface IPictureStorage
{
    /// <summary>False when the site has no storage configured; uploads are then switched off (PP07).</summary>
    bool IsAvailable { get; }

    Task PutAsync(string objectName, byte[] jpeg, CancellationToken cancellationToken);

    /// <summary>The bytes, or null when the object doesn't exist.</summary>
    Task<byte[]?> GetAsync(string objectName, CancellationToken cancellationToken);

    Task DeleteAsync(string objectName, CancellationToken cancellationToken);
}

/// <summary>Storage could not be reached or refused the request.</summary>
public sealed class PictureStorageException(string message, Exception? inner = null) : Exception(message, inner);

/// <summary>Which picture an account has, and whether it is public. Implemented in Data.</summary>
public interface IAccountPictureStore
{
    /// <summary>The account's role and current picture object, or null when the account doesn't exist.</summary>
    Task<AccountPicture?> FindAsync(Guid accountId, CancellationToken cancellationToken);

    /// <summary>Points the account at <paramref name="objectName"/> (null for none) and returns the previous object.</summary>
    Task<string?> SetAsync(Guid accountId, string? objectName, CancellationToken cancellationToken);

    /// <summary>Whether the client and the viewer have a conversation (F07): an accountant sees their clients' pictures.</summary>
    Task<bool> SharesConversationAsync(Guid clientAccountId, Guid viewerId, CancellationToken cancellationToken) => Task.FromResult(false);
}

public sealed record AccountPicture(bool IsAccountant, string? ObjectName);

/// <summary>A picture to send: its bytes, whether anyone may cache it, and its version.</summary>
public sealed record PictureFile(byte[] Jpeg, bool IsPublic, string Version);

public enum PictureUploadOutcome
{
    Saved,
    Rejected,
    Unavailable,
    Failed,
}

public sealed record PictureUploadResult(PictureUploadOutcome Outcome, PictureProblem? Problem = null);

/// <summary>
/// Profile pictures (PRD R24; F06). Every upload is processed before anything is stored; a new object is written,
/// then the account points at it, then the old object is deleted, so a failure never leaves the account pointing at
/// nothing. Reads apply the access rules: an accountant's picture is public, a client's only for that client.
/// </summary>
public sealed class ProfilePictureService(IPictureStorage storage, IAccountPictureStore accounts, ILogger<ProfilePictureService> logger)
{
    private static readonly EventId StorageFailed = new(1101, "PictureStorageFailed");

    public bool IsAvailable => storage.IsAvailable;

    /// <summary>The address of a picture, or null for none. The version changes with every upload, so it can be cached.</summary>
    public static string? AddressFor(Guid accountId, string? objectName) =>
        objectName is null ? null : $"/pictures/{accountId:D}?v={VersionOf(objectName)}";

    public async Task<PictureUploadResult> UploadAsync(Guid accountId, byte[] upload, CancellationToken cancellationToken = default)
    {
        if (!storage.IsAvailable)
        {
            return new PictureUploadResult(PictureUploadOutcome.Unavailable);
        }

        var processed = PictureProcessor.Process(upload);
        if (processed is PictureProcessResult.Rejected rejected)
        {
            return new PictureUploadResult(PictureUploadOutcome.Rejected, rejected.Problem);
        }

        var jpeg = ((PictureProcessResult.Ok)processed).Jpeg;
        var objectName = $"users/{accountId:N}/{Guid.NewGuid():N}.jpg";
        try
        {
            await storage.PutAsync(objectName, jpeg, cancellationToken);
            var previous = await accounts.SetAsync(accountId, objectName, cancellationToken);
            await DeleteQuietlyAsync(previous, cancellationToken);
            return new PictureUploadResult(PictureUploadOutcome.Saved);
        }
        catch (Exception ex) when (ex is PictureStorageException or DataUnavailableException)
        {
            logger.LogWarning(StorageFailed, "Picture upload failed ({ErrorType})", ex.GetType().Name);
            await DeleteQuietlyAsync(objectName, cancellationToken);
            return new PictureUploadResult(PictureUploadOutcome.Failed);
        }
    }

    /// <summary>Removes the account's picture; the initials avatar returns (PP04).</summary>
    public async Task<bool> RemoveAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        try
        {
            var previous = await accounts.SetAsync(accountId, null, cancellationToken);
            await DeleteQuietlyAsync(previous, cancellationToken);
            return true;
        }
        catch (DataUnavailableException)
        {
            return false;
        }
    }

    /// <summary>
    /// The picture for <paramref name="viewerId"/> to see (PP05): an accountant's picture for anyone, a client's only for
    /// that client; null for anything else, including a missing or unreadable picture (the page then shows initials).
    /// </summary>
    public async Task<PictureFile?> ReadAsync(Guid accountId, Guid? viewerId, CancellationToken cancellationToken = default)
    {
        if (!storage.IsAvailable)
        {
            return null;
        }

        try
        {
            if (await accounts.FindAsync(accountId, cancellationToken) is not { ObjectName: { } objectName } account
                || (!account.IsAccountant && viewerId != accountId
                    && !(viewerId is { } viewer && await accounts.SharesConversationAsync(accountId, viewer, cancellationToken))))
            {
                return null;
            }

            return await storage.GetAsync(objectName, cancellationToken) is { } jpeg
                ? new PictureFile(jpeg, account.IsAccountant, VersionOf(objectName))
                : null;
        }
        catch (Exception ex) when (ex is PictureStorageException or DataUnavailableException)
        {
            logger.LogWarning(StorageFailed, "Picture read failed ({ErrorType})", ex.GetType().Name);
            return null;
        }
    }

    private static string VersionOf(string objectName) => Path.GetFileNameWithoutExtension(objectName);

    private async Task DeleteQuietlyAsync(string? objectName, CancellationToken cancellationToken)
    {
        if (objectName is null)
        {
            return;
        }

        try
        {
            await storage.DeleteAsync(objectName, cancellationToken);
        }
        catch (PictureStorageException ex)
        {
            // An orphaned object is harmless (nothing points at it); it is logged for clean-up.
            logger.LogWarning(StorageFailed, "Picture delete failed ({ErrorType})", ex.GetType().Name);
        }
    }
}
