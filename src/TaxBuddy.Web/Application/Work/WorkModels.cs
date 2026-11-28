using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Application.Work;

/// <summary>A listed accountant as service work needs them: their account, name, title and offered services.</summary>
public sealed record WorkAccountant(AccountantId ProfileId, Guid AccountId, string Name, string Title, IReadOnlyList<string> Services);

/// <summary>One service on a list: name, status badge, checklist progress and its number of documents (R19, R20, R23).</summary>
public sealed record ServiceCard(Guid Id, string ServiceName, string StatusText, ServicePhase Phase, bool Completed, int Done, int Total, int Documents);

/// <summary>A row of the client's "My accountants": saved, or with services, or both.</summary>
public sealed record ClientAccountantEntry(
    AccountantId? ProfileId, Guid? AccountId, string Name, string? Title, bool Listed, bool Saved, Guid? ConversationId, IReadOnlyList<ServiceCard> Services);

public sealed record ClientWorkspaceView(IReadOnlyList<ClientAccountantEntry> Accountants)
{
    public int SavedCount => Accountants.Count(a => a.Saved);

    public int ActiveCount => Accountants.SelectMany(a => a.Services).Count(s => s.Phase == ServicePhase.Active && !s.Completed);

    public int CompletedCount => Accountants.SelectMany(a => a.Services).Count(s => s.Completed);
}

/// <summary>A row of the accountant's "Clients": requesting or engaged clients only (R19).</summary>
public sealed record AccountantClientEntry(Guid ClientId, string Name, int Services, int Requests);

public sealed record AccountantWorkspaceView(IReadOnlyList<AccountantClientEntry> Clients, int InProgress, int NewRequests);

/// <summary>An accountant's view of one client's services, and the services they could add (D24 proposal).</summary>
/// <summary>F03 client page; <paramref name="Details"/> is what the client shares with accountants they contact (F17, R25).</summary>
public sealed record ClientServicesView(Guid ClientId, string ClientName, Guid? ConversationId, IReadOnlyList<ServiceCard> Services, IReadOnlyList<string> CanAdd, ClientDetails Details);

public sealed record TaskView(Guid Id, string Title, ServiceTaskStatus Status);

public sealed record UpdateView(string AuthorName, DateTimeOffset At, string Text, bool IsNote);

/// <summary>A service document as the page lists it (R23): who added it, when, and to the whole service or a task.</summary>
public sealed record FileView(Guid Id, string Name, string UploaderName, DateTimeOffset At, string? TaskTitle, bool ViewerUploaded, long Length);

/// <summary>A service page (R21, R22): the checklist, progress and shared updates, as the viewer may see them.</summary>
public sealed record EngagementView(
    Guid Id,
    string ServiceName,
    ServicePhase Phase,
    string StatusText,
    bool Completed,
    Guid ClientId,
    string ClientName,
    string AccountantName,
    AccountantId? AccountantProfileId,
    bool ViewerIsAccountant,
    int Version,
    IReadOnlyList<TaskView> Tasks,
    IReadOnlyList<UpdateView> Updates,
    Guid? ConversationId,
    IReadOnlyList<FileView> Files,
    ClientDetails? ClientDetails)
{
    // ClientDetails (F17, R25): for the service's accountant only; null for the client, who sees their own on Account.

    public bool CanEdit => ViewerIsAccountant && Phase == ServicePhase.Active;

    /// <summary>R23: either participant attaches documents to accepted work.</summary>
    public bool CanAttach => Phase == ServicePhase.Active;

    public int Done => Tasks.Count(t => t.Status == ServiceTaskStatus.Completed);
}

public enum WorkOutcome
{
    Done,

    /// <summary>The same service is already requested or active for this pair (D24 proposal: no duplicates).</summary>
    Existing,

    /// <summary>The input breaks a rule; <see cref="WorkResult.Problem"/> says which.</summary>
    Invalid,

    /// <summary>A document upload refused because the service already holds the most documents it may (D26 working value).</summary>
    Full,

    /// <summary>Someone else saved a change first; the draft is kept and the current values shown (SDD).</summary>
    Stale,

    /// <summary>Not this viewer's to do, or doesn't exist: shown as unavailable, never with details.</summary>
    NotAllowed,
    Failed,
}

public sealed record WorkResult(WorkOutcome Outcome, Guid? EngagementId = null, string? Problem = null);

/// <summary>What the store changes in one transaction: the service's new state and the lines it adds to the history.</summary>
public delegate IReadOnlyList<(string Text, bool IsNote)>? EngagementChange(ServiceEngagement engagement);

/// <summary>
/// A static form's one-time key and the history line it writes (PRD D31, BR18). The same key with the same line from
/// the same person is the form sent again, and changes nothing; the same key with another line is a new post from an
/// older copy of the page (the browser's Back), and is stored without the key.
/// </summary>
public sealed record FormPost(Guid Key, string Line);

/// <summary>What an upload needs to know before storing bytes: the service's phase and tasks, and whether this form was already stored with this file and task.</summary>
public sealed record UploadTarget(ServicePhase Phase, IReadOnlyDictionary<Guid, string> Tasks, int Files, bool AlreadySent);

/// <summary>A stored document's object and download details, for one of its service's participants.</summary>
public sealed record StoredFile(string ObjectName, string DisplayName, string ContentType);

/// <summary>Service work on the database (SDD private service workspaces). A new context per operation.</summary>
public interface IWorkStore
{
    Task<WorkAccountant?> FindListedAsync(AccountantId profileId, CancellationToken cancellationToken);

    /// <summary>The accountant's own profile, listed or not, for the services they may add.</summary>
    Task<WorkAccountant?> FindProfileOfAsync(Guid accountantAccountId, CancellationToken cancellationToken);

    Task<bool> IsSavedAsync(Guid clientId, AccountantId profileId, CancellationToken cancellationToken);

    /// <summary>Listed in the directory, with or without an account behind it (saving is only a bookmark).</summary>
    Task<bool> IsListedAsync(AccountantId profileId, CancellationToken cancellationToken);

    /// <summary>Saves once; saving again changes nothing (R20).</summary>
    Task SaveAccountantAsync(Guid clientId, AccountantId profileId, DateTimeOffset at, CancellationToken cancellationToken);

    Task RemoveSavedAsync(Guid clientId, AccountantId profileId, CancellationToken cancellationToken);

    Task<ClientWorkspaceView> ClientWorkspaceAsync(Guid clientId, CancellationToken cancellationToken);

    Task<AccountantWorkspaceView> AccountantWorkspaceAsync(Guid accountantId, CancellationToken cancellationToken);

    /// <summary>Null when the accountant has no services with this client.</summary>
    Task<ClientServicesView?> ClientServicesAsync(Guid accountantId, Guid clientId, CancellationToken cancellationToken);

    /// <summary>The service for one of its two participants; null for anyone else or when it doesn't exist.</summary>
    Task<EngagementView?> ServiceAsync(Guid engagementId, Guid viewerId, CancellationToken cancellationToken);

    Task<ServiceEngagement?> FindAsync(Guid engagementId, CancellationToken cancellationToken);

    Task<Guid?> FindOpenAsync(Guid clientId, Guid accountantId, string serviceName, CancellationToken cancellationToken);

    Task<bool> HasActiveAsync(Guid clientId, Guid accountantId, CancellationToken cancellationToken);

    /// <summary>Adds a new service and its first history line; <see cref="WorkOutcome.Existing"/> when a duplicate won.</summary>
    Task<WorkResult> AddAsync(ServiceEngagement engagement, Guid authorId, string activity, CancellationToken cancellationToken);

    /// <summary>
    /// Loads the service, checks <paramref name="expectedVersion"/> when given, applies <paramref name="change"/> and its
    /// history lines by <paramref name="authorId"/>, and saves: Done, Stale, or NotAllowed when it is gone. A stale
    /// version is Done when the change finds nothing to change (the same form sent again), and Stale otherwise. With
    /// <paramref name="form"/>, the line it names carries the form's key, and Existing means that form was already stored.
    /// </summary>
    Task<WorkOutcome> ChangeAsync(Guid engagementId, int? expectedVersion, Guid authorId, EngagementChange change, DateTimeOffset at,
        FormPost? form, CancellationToken cancellationToken);

    /// <summary>The service as an upload by <paramref name="viewerId"/> sees it; null when it isn't theirs or doesn't exist.</summary>
    Task<UploadTarget?> UploadTargetAsync(Guid engagementId, Guid viewerId, Guid operationId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds the document and its history line in one transaction, checking again that the uploader is a participant
    /// with an account, the service is active, the task is the service's and there is room: Done, Existing when this
    /// form was already stored, Full, or NotAllowed.
    /// </summary>
    Task<WorkOutcome> AddFileAsync(ServiceFile file, string activity, CancellationToken cancellationToken);

    /// <summary>A document of the service, for one of its two participants; null for anyone else or when missing.</summary>
    Task<StoredFile?> FileAsync(Guid engagementId, Guid fileId, Guid viewerId, CancellationToken cancellationToken);

    /// <summary>Removes the uploader's own document and records it in the history; the removed object's name, or null when not theirs.</summary>
    Task<string?> RemoveFileAsync(Guid engagementId, Guid fileId, Guid uploaderId, DateTimeOffset at, CancellationToken cancellationToken);
}
