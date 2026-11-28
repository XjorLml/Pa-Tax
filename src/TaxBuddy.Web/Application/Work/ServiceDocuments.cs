using System.Security.Cryptography;
using SkiaSharp;
using TaxBuddy.Web.Application.Chat;
using TaxBuddy.Web.Application.Directory;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Application.Work;

/// <summary>Where service document bytes are kept: a private Supabase Storage bucket (D28, 2026-11-23). Implemented in Data.</summary>
public interface IDocumentStorage
{
    /// <summary>False when the site has no storage configured: uploads are then switched off.</summary>
    bool IsAvailable { get; }

    Task PutAsync(string objectName, byte[] content, string contentType, CancellationToken cancellationToken);

    /// <summary>The bytes, or null when the object doesn't exist.</summary>
    Task<byte[]?> GetAsync(string objectName, CancellationToken cancellationToken);

    Task DeleteAsync(string objectName, CancellationToken cancellationToken);
}

/// <summary>Document storage could not be reached or refused the request.</summary>
public sealed class DocumentStorageException(string message, Exception? inner = null) : Exception(message, inner);

public enum UploadProblem
{
    NoFile,
    TooLarge,
    WrongType,
    NoSuchTask,
    Full,
    Unavailable,
    Failed,
}

/// <summary>Done (stored, or already stored by the same form), NotAllowed (not the viewer's), or Invalid with the problem.</summary>
public sealed record UploadResult(WorkOutcome Outcome, UploadProblem? Problem = null);

public enum DownloadOutcome
{
    Ready,
    NotAllowed,
    Failed,
}

public sealed record DocumentDownload(DownloadOutcome Outcome, byte[]? Content = null, string? Name = null, string? ContentType = null);

/// <summary>
/// Service documents (F03 stage 2; PRD R23, BR15, BR17, BR18; D26–D28). Either participant attaches a PDF, JPEG or PNG
/// of up to 10 MB to accepted work, checked by its content; both can download it; only the uploader removes it. The
/// bytes are written first and the row second, so a document is listed only once it can be downloaded, and a failed
/// row write deletes the bytes again. There is no virus scanning (D28). Logs carry no names, IDs or file details.
/// </summary>
public sealed class ServiceDocuments(IWorkStore store, IDocumentStorage storage, IClock clock, ILogger<ServiceDocuments> logger)
{
    private static readonly EventId StorageFailed = new(1601, "DocumentStorageFailed");
    private static readonly EventId LeftInStorage = new(1602, "DocumentLeftInStorage");

    public bool IsAvailable => storage.IsAvailable;

    /// <summary>The object name for a new document: opaque, and grouped by service for operators.</summary>
    public static string ObjectNameFor(Guid engagementId, Guid fileId) => $"services/{engagementId:N}/{fileId:N}";

    /// <summary>The allowed type the bytes really are (D26), or null: a PDF with its end marker, or a JPEG or PNG an image decoder can read.</summary>
    public static string? ContentTypeOf(byte[] content)
    {
        var type = DocumentRules.TypeBySignature(content);
        return type switch
        {
            DocumentRules.Pdf => content.AsSpan(Math.Max(0, content.Length - 2048)).IndexOf("%%EOF"u8) >= 0 ? type : null,
            DocumentRules.Jpeg => IsImage(content, SKEncodedImageFormat.Jpeg) ? type : null,
            DocumentRules.Png => IsImage(content, SKEncodedImageFormat.Png) ? type : null,
            _ => null,
        };
    }

    /// <summary>
    /// The key a document is stored under: the upload form's one-time key, the chosen task and the file's content
    /// together (PRD D31, BR18). The same form sent again with the same file and task is one document. Another file or
    /// task sent from an older copy of the page (the browser's Back) is a new document, not a repeat to ignore.
    /// </summary>
    public static Guid UploadKey(Guid formKey, Guid? taskId, byte[] content)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        hash.AppendData(formKey.ToByteArray());
        hash.AppendData((taskId ?? Guid.Empty).ToByteArray());
        hash.AppendData(content);
        return new Guid(hash.GetHashAndReset().AsSpan(0, 16));
    }

    public async Task<UploadResult> AttachAsync(ChatViewer? viewer, Guid engagementId, Guid operationId, Guid? taskId, string? fileName, byte[] content,
        CancellationToken cancellationToken = default)
    {
        if (viewer is null)
        {
            return new UploadResult(WorkOutcome.NotAllowed);
        }

        var storedKey = UploadKey(operationId, taskId, content);
        UploadTarget? target;
        try
        {
            target = await store.UploadTargetAsync(engagementId, viewer.AccountId, storedKey, cancellationToken);
        }
        catch (DataUnavailableException)
        {
            return Invalid(UploadProblem.Failed);
        }

        if (target is null || target.Phase != ServicePhase.Active)
        {
            return new UploadResult(WorkOutcome.NotAllowed);
        }

        if (target.AlreadySent)
        {
            return new UploadResult(WorkOutcome.Done); // the same form again: the first one was stored (BR18)
        }

        if (!storage.IsAvailable)
        {
            return Invalid(UploadProblem.Unavailable);
        }

        if (content.Length == 0)
        {
            return Invalid(UploadProblem.NoFile);
        }

        if (content.Length > DocumentRules.MaxBytes)
        {
            return Invalid(UploadProblem.TooLarge);
        }

        if (ContentTypeOf(content) is not { } contentType)
        {
            return Invalid(UploadProblem.WrongType);
        }

        string? taskTitle = null;
        if (taskId is { } task && !target.Tasks.TryGetValue(task, out taskTitle))
        {
            return Invalid(UploadProblem.NoSuchTask);
        }

        if (target.Files >= DocumentRules.FilesPerService)
        {
            return Invalid(UploadProblem.Full);
        }

        var fileId = Guid.NewGuid();
        var name = DocumentRules.SafeName(fileName, contentType);
        var objectName = ObjectNameFor(engagementId, fileId);
        try
        {
            await storage.PutAsync(objectName, content, contentType, cancellationToken);
        }
        catch (DocumentStorageException ex)
        {
            logger.LogWarning(StorageFailed, "Document upload failed in storage ({ErrorType})", ex.GetType().Name);
            return Invalid(UploadProblem.Failed);
        }

        var file = new ServiceFile(fileId, engagementId, taskId, viewer.AccountId, storedKey, objectName, name, contentType, content.Length, clock.UtcNow);
        WorkOutcome outcome;
        try
        {
            outcome = await store.AddFileAsync(file, taskTitle is null ? $"Attached {name} to the service." : $"Attached {name} to {taskTitle}.", cancellationToken);
        }
        catch (DataUnavailableException)
        {
            // The row may or may not have been written (a timeout doesn't prove failure). The bytes go only when it surely
            // wasn't: an unlisted object is better than a listed document that can't be downloaded.
            switch (await StoredAsync(engagementId, fileId, viewer.AccountId))
            {
                case false:
                    await RemoveBytesAsync(objectName);
                    break;
                case null:
                    logger.LogWarning(LeftInStorage, "An uploaded document may have stayed in storage unlisted");
                    break;
            }

            return Invalid(UploadProblem.Failed);
        }

        if (outcome != WorkOutcome.Done)
        {
            await RemoveBytesAsync(objectName);
        }

        return outcome switch
        {
            WorkOutcome.Done or WorkOutcome.Existing => new UploadResult(WorkOutcome.Done),
            WorkOutcome.Full => Invalid(UploadProblem.Full),
            _ => new UploadResult(WorkOutcome.NotAllowed),
        };
    }

    /// <summary>A document for one of its service's participants (BR15); anyone else, or an unknown ID, gets NotAllowed.</summary>
    public async Task<DocumentDownload> DownloadAsync(ChatViewer? viewer, Guid engagementId, Guid fileId, CancellationToken cancellationToken = default)
    {
        if (viewer is null)
        {
            return new DocumentDownload(DownloadOutcome.NotAllowed);
        }

        try
        {
            if (await store.FileAsync(engagementId, fileId, viewer.AccountId, cancellationToken) is not { } file)
            {
                return new DocumentDownload(DownloadOutcome.NotAllowed);
            }

            if (await storage.GetAsync(file.ObjectName, cancellationToken) is not { } content)
            {
                logger.LogWarning(StorageFailed, "A listed document is missing from storage");
                return new DocumentDownload(DownloadOutcome.Failed);
            }

            return new DocumentDownload(DownloadOutcome.Ready, content, file.DisplayName, file.ContentType);
        }
        catch (DataUnavailableException)
        {
            return new DocumentDownload(DownloadOutcome.Failed);
        }
        catch (DocumentStorageException ex)
        {
            logger.LogWarning(StorageFailed, "Document download failed in storage ({ErrorType})", ex.GetType().Name);
            return new DocumentDownload(DownloadOutcome.Failed);
        }
    }

    /// <summary>D27 proposal: only the uploader removes a document. The row goes first, then the bytes.</summary>
    public async Task<WorkOutcome> RemoveAsync(ChatViewer? viewer, Guid engagementId, Guid fileId, CancellationToken cancellationToken = default)
    {
        if (viewer is null)
        {
            return WorkOutcome.NotAllowed;
        }

        string? objectName;
        try
        {
            objectName = await store.RemoveFileAsync(engagementId, fileId, viewer.AccountId, clock.UtcNow, cancellationToken);
        }
        catch (DataUnavailableException)
        {
            return WorkOutcome.Failed;
        }

        if (objectName is null)
        {
            return WorkOutcome.NotAllowed;
        }

        await RemoveBytesAsync(objectName);
        return WorkOutcome.Done;
    }

    /// <summary>After the row is gone: a failure leaves an unlisted object for the operator to delete (RELEASE).</summary>
    public async Task RemoveBytesAsync(string objectName)
    {
        try
        {
            await storage.DeleteAsync(objectName, CancellationToken.None);
        }
        catch (DocumentStorageException ex)
        {
            logger.LogWarning(LeftInStorage, "A removed service document stayed in storage ({ErrorType})", ex.GetType().Name);
        }
    }

    private async Task<bool?> StoredAsync(Guid engagementId, Guid fileId, Guid viewerId)
    {
        try
        {
            return await store.FileAsync(engagementId, fileId, viewerId, CancellationToken.None) is not null;
        }
        catch (DataUnavailableException)
        {
            return null;
        }
    }

    private static UploadResult Invalid(UploadProblem problem) => new(WorkOutcome.Invalid, problem);

    private static bool IsImage(byte[] content, SKEncodedImageFormat format)
    {
        using var data = SKData.CreateCopy(content);
        using var codec = SKCodec.Create(data);
        return codec is not null && codec.EncodedFormat == format && codec.Info.Width > 0 && codec.Info.Height > 0;
    }
}
