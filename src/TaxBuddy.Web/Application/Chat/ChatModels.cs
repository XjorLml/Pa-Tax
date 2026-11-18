using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Application.Chat;

/// <summary>Who is using a chat page: their account and its one role (PRD D03).</summary>
public sealed record ChatViewer(Guid AccountId, AccountRole Role)
{
    public bool IsClient => Role == AccountRole.Client;
}

/// <summary>A row of the conversation list (prototype <c>convListHTML</c>).</summary>
public sealed record ConversationSummary(
    Guid Id,
    string PeerName,
    string? PeerPicture,
    string? LastText,
    bool LastIsMine,
    DateTimeOffset? LastAt,
    int Unread);

public sealed record MessageView(Guid Id, bool Mine, string Text, DateTimeOffset SentAt);

/// <summary>A conversation as one participant sees it. <see cref="PeerProfileId"/> is set for a listed accountant.</summary>
public sealed record ThreadView(
    Guid ConversationId,
    bool ViewerIsClient,
    string PeerName,
    string? PeerPicture,
    AccountantId? PeerProfileId,
    IReadOnlyList<MessageView> Messages)
{
    /// <summary>The viewer has blocked the other participant (R15): "You blocked …" with Unblock instead of the composer.</summary>
    public bool BlockedByMe { get; init; }

    /// <summary>The other participant has blocked the viewer: "You can't send messages in this conversation." Not told why.</summary>
    public bool BlockedByThem { get; init; }

    public bool IsBlocked => BlockedByMe || BlockedByThem;
}

/// <summary>The accountant a new chat goes to.</summary>
public sealed record ChatRecipient(AccountantId ProfileId, Guid AccountId, string Name, string? Picture);

public enum NewChatOutcome
{
    /// <summary>Ready to write the first message.</summary>
    Ready,

    /// <summary>The pair already has a conversation (D06): go there.</summary>
    Existing,

    /// <summary>The profile isn't listed or doesn't exist.</summary>
    Unavailable,

    /// <summary>Listed, but no account receives its messages (a profile created without an account).</summary>
    NoAccount,

    /// <summary>Only clients start chats (BR03).</summary>
    NotAClient,
}

public sealed record NewChat(NewChatOutcome Outcome, ChatRecipient? Recipient = null, Guid? ConversationId = null);

public enum SendOutcome
{
    Sent,
    Invalid,
    TooFast,
    NotAllowed,

    /// <summary>One of the participants has blocked the other (R15); nothing was stored.</summary>
    Blocked,
    Failed,
}

public sealed record SendResult(SendOutcome Outcome, Guid? ConversationId = null, MessageView? Message = null, MessageProblem? Problem = null);

/// <summary>Raw data the store reads; the service turns it into views.</summary>
public sealed record ConversationRow(
    Guid Id,
    Guid PeerAccountId,
    string PeerName,
    string? PeerPictureObject,
    string? LastText,
    Guid? LastSenderId,
    DateTimeOffset? LastAt,
    int Unread);

public sealed record ThreadData(
    Conversation Conversation,
    string PeerName,
    string? PeerPictureObject,
    AccountantId? PeerListedProfileId,
    IReadOnlyList<ChatMessage> Messages);

public enum StoreMessageOutcome
{
    Stored,

    /// <summary>A message with this ID was already stored (a repeated send).</summary>
    AlreadyStored,

    /// <summary>The conversation is blocked (R15); nothing was stored.</summary>
    Blocked,
}

public sealed record StoredMessage(StoreMessageOutcome Outcome, ChatMessage Message, Conversation Conversation);

/// <summary>Conversations and messages on PostgreSQL. Implemented in Data; failures become <c>DataUnavailableException</c>.</summary>
public interface IChatStore
{
    /// <summary>A listed profile and the account that owns it (null owner when none), or null when not listed.</summary>
    Task<(AccountantId ProfileId, Guid? OwnerAccountId, string Name, string? PictureObject)?> FindListedAccountantAsync(
        AccountantId profileId, CancellationToken cancellationToken);

    Task<Guid?> FindConversationIdAsync(Guid clientAccountId, Guid accountantAccountId, CancellationToken cancellationToken);

    Task<Conversation?> FindAsync(Guid conversationId, CancellationToken cancellationToken);

    Task<ChatMessage?> FindMessageAsync(Guid messageId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ConversationRow>> ListAsync(Guid accountId, CancellationToken cancellationToken);

    Task<ThreadData?> LoadThreadAsync(Guid conversationId, Guid viewerId, int limit, CancellationToken cancellationToken);

    /// <summary>
    /// Stores the message and moves the conversation forward, in one transaction. With <paramref name="newPair"/>, the
    /// conversation for that client and accountant is found or created first (one per pair, D06).
    /// </summary>
    Task<StoredMessage> AddMessageAsync(
        ChatMessage message, (Guid ClientAccountId, Guid AccountantAccountId)? newPair, CancellationToken cancellationToken);

    Task<int> CountSentSinceAsync(Guid senderAccountId, DateTimeOffset since, CancellationToken cancellationToken);

    Task MarkReadAsync(Guid conversationId, Guid readerId, DateTimeOffset upTo, CancellationToken cancellationToken);

    Task<int> UnreadTotalAsync(Guid accountId, CancellationToken cancellationToken);

    /// <summary>
    /// Sets or clears the participant's own block (R15) under a row lock, so a message being stored at the same moment either
    /// lands before the block or is refused. Null when the conversation doesn't exist or the account isn't in it.
    /// </summary>
    Task<Conversation?> SetBlockAsync(Guid conversationId, Guid accountId, bool blocked, DateTimeOffset at, CancellationToken cancellationToken);
}

/// <summary>
/// Tells open chat pages that a conversation changed, so they reload it from the database (SDD live updates). One server
/// instance; implemented in Services.
/// </summary>
public interface IChatNotifier
{
    /// <summary>Calls <paramref name="changed"/> with the conversation ID whenever one of the account's conversations changes.</summary>
    IDisposable Subscribe(Guid accountId, Func<Guid, Task> changed);

    void Changed(Conversation conversation);
}
