namespace TaxBuddy.Web.Domain;

/// <summary>Review limits (PRD R13, R14; D23 provisionally decided 2026-11-17 from the prototype).</summary>
public static class ReviewLimits
{
    public const int MinStars = 1;

    public const int MaxStars = 5;

    /// <summary>The longest written review, in characters after trimming. Writing one is optional; there is no minimum.</summary>
    public const int Text = 1000;

    /// <summary>The longest reply, in characters after trimming. A reply can't be empty.</summary>
    public const int Reply = 600;
}

/// <summary>Review and reply text: trimmed, line endings as <c>\n</c>; empty becomes null.</summary>
public static class ReviewText
{
    public static string? Normalize(string? text)
    {
        var trimmed = (text ?? "").Replace("\r\n", "\n").Replace('\r', '\n').Trim();
        return trimmed.Length == 0 ? null : trimmed;
    }
}

/// <summary>How a reviewer is named publicly (PRD R11, D21 provisionally decided 2026-11-17): first name and last initial.</summary>
public static class ReviewerName
{
    public const string Fallback = "A client";

    /// <summary>"Liza Fernandez" → "Liza F."; "Liza" → "Liza"; nothing → "A client".</summary>
    public static string Short(string? displayName)
    {
        var words = (displayName ?? "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return words.Length switch
        {
            0 => Fallback,
            1 => words[0],
            _ => $"{words[0]} {char.ToUpperInvariant(words[^1][0])}.",
        };
    }
}

/// <summary>
/// A client's rating of an accountant, with optional text and the accountant's one public reply (PRD R13, R14). One per
/// client and accountant (BR09), so the client changes or deletes it rather than adding another. Only its client changes it,
/// and only the reviewed accountant replies (BR11); the Application service checks who is acting.
/// </summary>
public sealed class Review
{
    private Review()
    {
    }

    public Review(Guid id, AccountantId accountantId, Guid clientAccountId, int stars, string? text, DateTimeOffset at)
    {
        Id = id;
        AccountantId = accountantId;
        ClientAccountId = clientAccountId;
        CreatedAt = at;
        Change(stars, text, at);
    }

    public Guid Id { get; private set; }

    /// <summary>The reviewed profile.</summary>
    public AccountantId AccountantId { get; private set; }

    public Guid ClientAccountId { get; private set; }

    public int Stars { get; private set; }

    public string? Text { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>The last change by the client. Reviews are listed newest first by this time, as in the prototype.</summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    public string? ReplyText { get; private set; }

    public DateTimeOffset? ReplyAt { get; private set; }

    /// <summary>New stars and text from the client. The reply stays.</summary>
    public void Change(int stars, string? text, DateTimeOffset at)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(stars, ReviewLimits.MinStars);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(stars, ReviewLimits.MaxStars);
        var normalized = ReviewText.Normalize(text);
        if (normalized?.Length > ReviewLimits.Text)
        {
            throw new ArgumentException($"A review can be at most {ReviewLimits.Text} characters.", nameof(text));
        }

        Stars = stars;
        Text = normalized;
        UpdatedAt = at;
    }

    /// <summary>Posts or replaces the accountant's reply (R14: one per review, editable).</summary>
    public void SetReply(string? text, DateTimeOffset at)
    {
        var normalized = ReviewText.Normalize(text) ?? throw new ArgumentException("A reply can't be empty.", nameof(text));
        if (normalized.Length > ReviewLimits.Reply)
        {
            throw new ArgumentException($"A reply can be at most {ReviewLimits.Reply} characters.", nameof(text));
        }

        ReplyText = normalized;
        ReplyAt = at;
    }
}
