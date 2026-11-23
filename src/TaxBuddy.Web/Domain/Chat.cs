namespace TaxBuddy.Web.Domain;

/// <summary>Chat limits (PRD D06, decided 2026-11-16; SDD abuse limits).</summary>
public static class ChatLimits
{
    /// <summary>The longest message, in characters after trimming (D06).</summary>
    public const int MessageLength = 2000;

    /// <summary>Messages one account may send in a minute, across all its conversations.</summary>
    public const int MessagesPerMinute = 20;

    /// <summary>How many recent messages a thread shows.</summary>
    public const int ThreadMessages = 200;
}

public enum MessageProblem
{
    Empty,
    TooLong,
}

/// <summary>The message text rule (PRD BR04): plain text, trimmed, not empty, at most <see cref="ChatLimits.MessageLength"/>.</summary>
public static class MessageText
{
    /// <summary>The text to store, or the problem with it. Line endings become <c>\n</c>.</summary>
    public static (string? Text, MessageProblem? Problem) Normalize(string? text)
    {
        var trimmed = (text ?? "").Replace("\r\n", "\n").Replace('\r', '\n').Trim();
        if (trimmed.Length == 0)
        {
            return (null, MessageProblem.Empty);
        }

        return trimmed.Length > ChatLimits.MessageLength ? (null, MessageProblem.TooLong) : (trimmed, null);
    }
}

/// <summary>
/// A conversation between one client and one accountant (PRD R06, R07). One per pair (D06). Each side's read time marks
/// what it has seen: the other side's later messages are unread.
/// </summary>
public sealed class Conversation
{
    private Conversation()
    {
    }

    public Conversation(Guid id, Guid clientAccountId, Guid accountantAccountId, DateTimeOffset createdAt)
    {
        Id = id;
        ClientAccountId = clientAccountId;
        AccountantAccountId = accountantAccountId;
        CreatedAt = createdAt;
        LastMessageAt = createdAt;
    }

    public Guid Id { get; private set; }

    public Guid ClientAccountId { get; private set; }

    public Guid AccountantAccountId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset LastMessageAt { get; private set; }

    public DateTimeOffset? ClientReadAt { get; private set; }

    public DateTimeOffset? AccountantReadAt { get; private set; }

    /// <summary>When the client blocked the accountant (R15, F10); null when not blocked.</summary>
    public DateTimeOffset? ClientBlockedAt { get; private set; }

    /// <summary>When the accountant blocked the client (R15, F10); null when not blocked.</summary>
    public DateTimeOffset? AccountantBlockedAt { get; private set; }

    /// <summary>While either side has blocked the other, neither can send here (R15).</summary>
    public bool IsBlocked => ClientBlockedAt is not null || AccountantBlockedAt is not null;

    /// <summary>Whether <paramref name="accountId"/> has blocked the other participant.</summary>
    public bool IsBlockedBy(Guid accountId) =>
        accountId == ClientAccountId ? ClientBlockedAt is not null : accountId == AccountantAccountId && AccountantBlockedAt is not null;

    public bool Includes(Guid accountId) => accountId == ClientAccountId || accountId == AccountantAccountId;

    /// <summary>The participant blocks the other. Blocking again keeps the first time.</summary>
    public void Block(Guid accountId, DateTimeOffset at)
    {
        if (accountId == ClientAccountId)
        {
            ClientBlockedAt ??= at;
        }
        else if (accountId == AccountantAccountId)
        {
            AccountantBlockedAt ??= at;
        }
        else
        {
            throw new InvalidOperationException("Only a participant can block.");
        }
    }

    /// <summary>The participant undoes their own block; the other side's block, if any, stays.</summary>
    public void Unblock(Guid accountId)
    {
        if (accountId == ClientAccountId)
        {
            ClientBlockedAt = null;
        }
        else if (accountId == AccountantAccountId)
        {
            AccountantBlockedAt = null;
        }
        else
        {
            throw new InvalidOperationException("Only a participant can unblock.");
        }
    }

    public Guid OtherThan(Guid accountId) => accountId == ClientAccountId ? AccountantAccountId : ClientAccountId;

    /// <summary>When the participant last opened the conversation (F14 uses it to send one email until then).</summary>
    public DateTimeOffset? ReadAtFor(Guid accountId) => accountId == ClientAccountId ? ClientReadAt : AccountantReadAt;

    /// <summary>A new message: the conversation moves to the top, and the sender has read up to it.</summary>
    public void RecordMessage(Guid senderId, DateTimeOffset sentAt)
    {
        if (sentAt > LastMessageAt)
        {
            LastMessageAt = sentAt;
        }

        MarkRead(senderId, sentAt);
    }

    /// <summary>Moves the reader's read time forward to <paramref name="upTo"/>; never back.</summary>
    public bool MarkRead(Guid readerId, DateTimeOffset upTo)
    {
        if (readerId == ClientAccountId && (ClientReadAt is null || upTo > ClientReadAt))
        {
            ClientReadAt = upTo;
            return true;
        }

        if (readerId == AccountantAccountId && (AccountantReadAt is null || upTo > AccountantReadAt))
        {
            AccountantReadAt = upTo;
            return true;
        }

        return false;
    }
}

/// <summary>
/// A message (BR04). Its ID is made when the composer is shown, so sending the same message twice stores it once
/// (SDD identity and mutation).
/// </summary>
public sealed class ChatMessage
{
    private ChatMessage()
    {
    }

    public ChatMessage(Guid id, Guid conversationId, Guid senderAccountId, string text, DateTimeOffset sentAt)
    {
        Id = id;
        ConversationId = conversationId;
        SenderAccountId = senderAccountId;
        Text = text;
        SentAt = sentAt;
    }

    public Guid Id { get; private set; }

    public Guid ConversationId { get; private set; }

    public Guid SenderAccountId { get; private set; }

    public string Text { get; private set; } = "";

    public DateTimeOffset SentAt { get; private set; }
}
