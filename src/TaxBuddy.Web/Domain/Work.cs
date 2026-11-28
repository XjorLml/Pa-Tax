namespace TaxBuddy.Web.Domain;

/// <summary>Where a requested service stands (PRD R21, BR14; D24 prototype proposals adopted 2026-11-23).</summary>
public enum ServicePhase
{
    Requested,
    Active,
    Declined,
}

/// <summary>A checklist task's status (PRD BR16; D25 prototype proposal adopted 2026-11-23).</summary>
public enum ServiceTaskStatus
{
    NotStarted,
    InProgress,
    WaitingForClient,
    Completed,
}

/// <summary>A client's private bookmark of a listed accountant (R20, BR14). Saving shares nothing with the accountant.</summary>
public sealed class SavedAccountant
{
    private SavedAccountant()
    {
    }

    public SavedAccountant(Guid clientAccountId, AccountantId profileId, DateTimeOffset savedAt)
    {
        ClientAccountId = clientAccountId;
        ProfileId = profileId;
        SavedAt = savedAt;
    }

    public Guid ClientAccountId { get; private set; }

    public AccountantId ProfileId { get; private set; }

    public DateTimeOffset SavedAt { get; private set; }
}

/// <summary>
/// One service between a client and an accountant (R19–R22, BR14–BR16). It starts as a client's request (sharing the
/// client's name and the service with that accountant), and becomes shared work when the accountant accepts it, with a
/// starting checklist. The accountant may also add a service for a client they already work with (D24 proposal).
/// </summary>
public sealed class ServiceEngagement
{
    private readonly List<ServiceTask> tasks = [];

    private ServiceEngagement()
    {
    }

    private ServiceEngagement(Guid id, Guid clientAccountId, Guid accountantAccountId, string serviceName, ServicePhase phase, DateTimeOffset at)
    {
        Id = id;
        ClientAccountId = clientAccountId;
        AccountantAccountId = accountantAccountId;
        ServiceName = serviceName;
        Phase = phase;
        CreatedAt = at;
        UpdatedAt = at;
    }

    public Guid Id { get; private set; }

    public Guid ClientAccountId { get; private set; }

    public Guid AccountantAccountId { get; private set; }

    /// <summary>The service's name when it was requested (from the fixed list, D04).</summary>
    public string ServiceName { get; private set; } = "";

    public ServicePhase Phase { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>Optimistic concurrency for checklist saves (SDD: a stale save is refused with the draft kept).</summary>
    public int Version { get; private set; }

    public IReadOnlyList<ServiceTask> Tasks => tasks;

    public static ServiceEngagement Request(Guid clientAccountId, Guid accountantAccountId, string serviceName, DateTimeOffset at) =>
        new(Guid.NewGuid(), clientAccountId, accountantAccountId, serviceName, ServicePhase.Requested, at);

    /// <summary>An accountant adding work for a client they already serve (D24 proposal): active at once, with the starting checklist.</summary>
    public static ServiceEngagement AddedByAccountant(Guid clientAccountId, Guid accountantAccountId, string serviceName, DateTimeOffset at)
    {
        var engagement = new ServiceEngagement(Guid.NewGuid(), clientAccountId, accountantAccountId, serviceName, ServicePhase.Active, at);
        engagement.AddStartingTasks(at);
        return engagement;
    }

    public bool Includes(Guid accountId) => accountId == ClientAccountId || accountId == AccountantAccountId;

    public void Accept(DateTimeOffset at)
    {
        if (Phase != ServicePhase.Requested)
        {
            throw new InvalidOperationException("Only a request can be accepted.");
        }

        Phase = ServicePhase.Active;
        AddStartingTasks(at);
        Touch(at);
    }

    public void Decline(DateTimeOffset at)
    {
        if (Phase != ServicePhase.Requested)
        {
            throw new InvalidOperationException("Only a request can be declined.");
        }

        Phase = ServicePhase.Declined;
        Touch(at);
    }

    public ServiceTask AddTask(string title, DateTimeOffset at)
    {
        var task = new ServiceTask(Guid.NewGuid(), Id, title, tasks.Count == 0 ? 0 : tasks.Max(t => t.Position) + 1, at);
        tasks.Add(task);
        Touch(at);
        return task;
    }

    /// <summary>Changes a task's status; false when nothing changed.</summary>
    public bool SetStatus(Guid taskId, ServiceTaskStatus status, DateTimeOffset at)
    {
        var task = tasks.Single(t => t.Id == taskId);
        if (task.Status == status)
        {
            return false;
        }

        task.Status = status;
        Touch(at);
        return true;
    }

    /// <summary>BR16: completed tasks out of all tasks; no tasks is never "complete".</summary>
    public (int Done, int Total) Progress => (tasks.Count(t => t.Status == ServiceTaskStatus.Completed), tasks.Count);

    /// <summary>The prototype's one-word summary of a service (status badge).</summary>
    public string StatusText => Phase switch
    {
        ServicePhase.Requested => "Awaiting acceptance",
        ServicePhase.Declined => "Declined",
        _ when tasks.Count == 0 => "Not started",
        _ when tasks.All(t => t.Status == ServiceTaskStatus.Completed) => "Completed",
        _ when tasks.Any(t => t.Status == ServiceTaskStatus.WaitingForClient) => "Waiting for client",
        _ when tasks.Any(t => t.Status != ServiceTaskStatus.NotStarted) => "In progress",
        _ => "Not started",
    };

    public bool IsCompleted => Phase == ServicePhase.Active && tasks.Count > 0 && tasks.All(t => t.Status == ServiceTaskStatus.Completed);

    private void AddStartingTasks(DateTimeOffset at)
    {
        foreach (var title in WorkRules.StartingTasks)
        {
            AddTask(title, at);
        }
    }

    private void Touch(DateTimeOffset at)
    {
        UpdatedAt = at;
        Version++;
    }
}

public sealed class ServiceTask
{
    private ServiceTask()
    {
    }

    internal ServiceTask(Guid id, Guid engagementId, string title, int position, DateTimeOffset createdAt)
    {
        Id = id;
        EngagementId = engagementId;
        Title = title;
        Position = position;
        CreatedAt = createdAt;
        Status = ServiceTaskStatus.NotStarted;
    }

    public Guid Id { get; private set; }

    public Guid EngagementId { get; private set; }

    public string Title { get; private set; } = "";

    public ServiceTaskStatus Status { get; internal set; }

    /// <summary>Stable order: the order tasks were added.</summary>
    public int Position { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
}

/// <summary>A shared progress note or an activity line on a service (R22): author and time are always shown.</summary>
public sealed class ServiceUpdate
{
    private ServiceUpdate()
    {
    }

    public ServiceUpdate(Guid id, Guid engagementId, Guid authorAccountId, string text, bool isNote, DateTimeOffset at, Guid? formKey = null)
    {
        Id = id;
        EngagementId = engagementId;
        AuthorAccountId = authorAccountId;
        Text = text;
        IsNote = isNote;
        CreatedAt = at;
        FormKey = formKey;
    }

    public Guid Id { get; private set; }

    public Guid EngagementId { get; private set; }

    public Guid AuthorAccountId { get; private set; }

    public string Text { get; private set; } = "";

    /// <summary>True for a note the accountant wrote; false for an activity line the site wrote.</summary>
    public bool IsNote { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// The one-time key of the static form that wrote this line ("Add task", the shared note), unique per service: the
    /// same form sent again, by a double click or a retry, changes nothing (BR18, PRD D31). Null for other lines.
    /// </summary>
    public Guid? FormKey { get; private set; }
}

/// <summary>
/// A private document on a service (R23, BR17): it belongs to one service and optionally one of its tasks, and only the
/// service's two participants can download it. A row exists only once its bytes are stored, so every row is available.
/// The object name is opaque and never comes from the uploaded file's name.
/// </summary>
public sealed class ServiceFile
{
    private ServiceFile()
    {
    }

    public ServiceFile(Guid id, Guid engagementId, Guid? taskId, Guid uploaderAccountId, Guid operationId, string objectName,
        string displayName, string contentType, long length, DateTimeOffset createdAt)
    {
        Id = id;
        EngagementId = engagementId;
        TaskId = taskId;
        UploaderAccountId = uploaderAccountId;
        OperationId = operationId;
        ObjectName = objectName;
        DisplayName = displayName;
        ContentType = contentType;
        Length = length;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public Guid EngagementId { get; private set; }

    /// <summary>The checklist task it is attached to; null for the whole service.</summary>
    public Guid? TaskId { get; private set; }

    public Guid UploaderAccountId { get; private set; }

    /// <summary>
    /// Made from the upload form's one-time key, the task and the file's content: the same form sent twice with the same
    /// file (a retry or a double click) stores one file (BR18, PRD D31). Before 2026-11-25 it was the form's key alone.
    /// </summary>
    public Guid OperationId { get; private set; }

    public string ObjectName { get; private set; } = "";

    /// <summary>The uploaded file's name made safe to show and to offer as a download name.</summary>
    public string DisplayName { get; private set; } = "";

    /// <summary>The type found in the file's content, not the one the browser claimed.</summary>
    public string ContentType { get; private set; } = "";

    public long Length { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
}

/// <summary>
/// Service document rules (R23; D26 decided 2026-11-23: PDF, JPEG and PNG, checked by content, up to 10 MB each).
/// The number per service is a working value: D26's count limits and quotas are still open.
/// </summary>
public static class DocumentRules
{
    public const long MaxBytes = 10 * 1024 * 1024;
    public const int FilesPerService = 50;
    public const int DisplayName = 120;

    public const string Pdf = "application/pdf";
    public const string Jpeg = "image/jpeg";
    public const string Png = "image/png";

    /// <summary>The file input's accept list: a hint for the browser's picker only; the server checks the content.</summary>
    public const string Accept = ".pdf,.jpg,.jpeg,.png,application/pdf,image/jpeg,image/png";

    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    /// <summary>The type the content's first bytes show, or null when it is none of the allowed ones.</summary>
    public static string? TypeBySignature(ReadOnlySpan<byte> content) =>
        content.StartsWith("%PDF-"u8) ? Pdf
        : content.Length >= 3 && content[0] == 0xFF && content[1] == 0xD8 && content[2] == 0xFF ? Jpeg
        : content.StartsWith(PngSignature) ? Png
        : null;

    /// <summary>
    /// The name to show and to download as: the last part of the uploaded name, without control or path characters,
    /// at most <see cref="DisplayName"/> characters, ending in an extension that matches the content.
    /// </summary>
    public static string SafeName(string? uploaded, string contentType)
    {
        var name = uploaded ?? "";
        name = name[(Math.Max(name.LastIndexOf('/'), name.LastIndexOf('\\')) + 1)..];
        name = new string(name.Where(c => !char.IsControl(c) && !"<>:\"|?*".Contains(c)
                                          && char.GetUnicodeCategory(c) is not (System.Globalization.UnicodeCategory.Format
                                              or System.Globalization.UnicodeCategory.LineSeparator
                                              or System.Globalization.UnicodeCategory.ParagraphSeparator))
            .ToArray()).Trim().Trim('.').Trim();

        string[] extensions = contentType switch
        {
            Pdf => [".pdf"],
            Jpeg => [".jpg", ".jpeg"],
            _ => [".png"],
        };
        var extension = Path.GetExtension(name);
        var stem = extensions.Contains(extension, StringComparer.OrdinalIgnoreCase) ? name[..^extension.Length] : name;
        if (!extensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            extension = extensions[0];
        }

        stem = stem.TrimEnd('.', ' ');
        if (stem.Length == 0)
        {
            stem = "document";
        }

        var room = DisplayName - extension.Length;
        if (stem.Length > room)
        {
            stem = stem[..room].TrimEnd('.', ' ');
            if (char.IsHighSurrogate(stem[^1]))
            {
                stem = stem[..^1];
            }
        }

        return stem + extension;
    }

    /// <summary>"240 KB" or "3.4 MB", for the file list.</summary>
    public static string SizeText(long length) =>
        length < 1024 * 1024
            ? FormattableString.Invariant($"{Math.Max(1, (int)Math.Ceiling(length / 1024.0))} KB")
            : FormattableString.Invariant($"{length / (1024.0 * 1024):0.#} MB");
}

/// <summary>Limits and labels for service work (F03; D25 prototype values adopted 2026-11-23).</summary>
public static class WorkRules
{
    public const int TaskTitle = 120;
    public const int Note = 1000;
    public const int TasksPerService = 30;

    public static readonly string[] StartingTasks = ["Agree the service requirements", "Prepare the work", "Review and hand over"];

    public static string Label(ServiceTaskStatus status) => status switch
    {
        ServiceTaskStatus.NotStarted => "Not started",
        ServiceTaskStatus.InProgress => "In progress",
        ServiceTaskStatus.WaitingForClient => "Waiting for client",
        _ => "Completed",
    };
}
