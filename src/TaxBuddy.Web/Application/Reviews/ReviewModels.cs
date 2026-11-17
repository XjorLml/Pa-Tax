using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Application.Reviews;

/// <summary>Whether the viewer may write a review of an accountant (PRD BR09), and if not, why (DESIGN: "cannot review, with the reason").</summary>
public enum ReviewAccess
{
    Allowed,

    /// <summary>A visitor: reviews come from clients the accountant has chatted with.</summary>
    SignIn,

    /// <summary>An accountant: only clients write reviews.</summary>
    NotAClient,

    /// <summary>A client the accountant hasn't sent a message to yet, including profiles without an account.</summary>
    NoReplyYet,
}

/// <summary>
/// The average and count, computed from the reviews (SDD ratings). The average is shown to one decimal from the first review
/// on; with no reviews there is no average at all, never a zero score (BR10).
/// </summary>
public sealed record RatingSummary(int Count, int StarTotal)
{
    public static RatingSummary None { get; } = new(0, 0);

    public decimal? Average => Count == 0 ? null : Math.Round((decimal)StarTotal / Count, 1, MidpointRounding.AwayFromZero);
}

/// <summary>A listed accountant who can be reviewed, with the account that replies (null for profiles without one).</summary>
public sealed record ReviewTarget(AccountantId ProfileId, Guid? OwnerAccountId, string Name);

/// <summary>The review form as posted (a static form, C04 closed for reviews 2026-11-17). Stars arrive as text and are checked.</summary>
public sealed class ReviewForm
{
    public string? Stars { get; set; }

    public string? Text { get; set; }
}

/// <summary>The accountant's reply as posted from their profile (a static named form, SDD render modes).</summary>
public sealed class ReplyForm
{
    public string? ReviewId { get; set; }

    public string? Text { get; set; }
}

/// <summary>The review page: who is reviewed, whether the viewer may, their public name and their saved review.</summary>
public sealed record ReviewFormPage(ReviewTarget Accountant, ReviewAccess Access, string? ReviewerName, ReviewForm? Saved);

public enum ReviewOutcome
{
    Posted,
    Updated,
    Deleted,
    Invalid,
    NotAllowed,

    /// <summary>The profile is no longer listed (BR01).</summary>
    Gone,

    /// <summary>The database couldn't be reached; nothing is known to be saved.</summary>
    Failed,
}

public sealed record ReviewResult(ReviewOutcome Outcome, IReadOnlyDictionary<string, string>? Errors = null, ReviewAccess? Access = null);

public enum ReplyOutcome
{
    Posted,
    Invalid,
    NotAllowed,
    Failed,
}

public sealed record ReplyResult(ReplyOutcome Outcome, string? Error = null);

/// <summary>A review as the profile shows it: the reviewer by short name only (R11, D21).</summary>
public sealed record ReviewView(
    Guid Id,
    int Stars,
    string ReviewerName,
    string? Text,
    DateTimeOffset At,
    bool IsMine,
    string? ReplyText,
    DateTimeOffset? ReplyAt);

/// <summary>The profile's Reviews section for one viewer.</summary>
public sealed record ProfileReviews(
    RatingSummary Rating,
    IReadOnlyList<ReviewView> Reviews,
    ReviewAccess Access,
    bool HasReview,
    bool IsOwner)
{
    public static ProfileReviews Empty { get; } = new(RatingSummary.None, [], ReviewAccess.SignIn, false, false);
}

/// <summary>A stored review with its author's account name, newest change first.</summary>
public sealed record ReviewRow(
    Guid Id,
    Guid ClientAccountId,
    string? ClientName,
    int Stars,
    string? Text,
    DateTimeOffset UpdatedAt,
    string? ReplyText,
    DateTimeOffset? ReplyAt);

/// <summary>Reviews on PostgreSQL. Implemented in Data; failures surface as <see cref="Directory.DataUnavailableException"/>.</summary>
public interface IReviewStore
{
    /// <summary>The accountant when the profile is listed (BR01); otherwise null.</summary>
    Task<ReviewTarget?> FindListedAsync(AccountantId profileId, CancellationToken cancellationToken);

    /// <summary>Whether the accountant has sent at least one message in their conversation with the client (BR09).</summary>
    Task<bool> HasAccountantRepliedAsync(Guid clientAccountId, Guid accountantAccountId, CancellationToken cancellationToken);

    Task<Review?> FindAsync(AccountantId profileId, Guid clientAccountId, CancellationToken cancellationToken);

    /// <summary>The account's display name, for the reviewer's public short name.</summary>
    Task<string?> AccountNameAsync(Guid accountId, CancellationToken cancellationToken);

    /// <summary>Creates the client's review of the accountant, or changes it when there is one. True when it changed one.</summary>
    Task<bool> SaveAsync(AccountantId profileId, Guid clientAccountId, int stars, string? text, DateTimeOffset at, CancellationToken cancellationToken);

    /// <summary>Deletes the client's review of the accountant, with its reply. False when there was none.</summary>
    Task<bool> DeleteAsync(AccountantId profileId, Guid clientAccountId, CancellationToken cancellationToken);

    /// <summary>Sets the reply on a review of this profile. False when the review isn't one of this profile's.</summary>
    Task<bool> SaveReplyAsync(AccountantId profileId, Guid reviewId, string text, DateTimeOffset at, CancellationToken cancellationToken);

    /// <summary>The profile's reviews, newest change first, then by ID.</summary>
    Task<IReadOnlyList<ReviewRow>> ListAsync(AccountantId profileId, CancellationToken cancellationToken);
}
