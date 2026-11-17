using System.Globalization;
using TaxBuddy.Web.Application.Chat;
using TaxBuddy.Web.Application.Directory;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Application.Reviews;

/// <summary>
/// Ratings, reviews and replies (PRD R13, R14, BR09–BR11; F09). Every write checks the acting account again: a client may
/// review an accountant only after that accountant has sent them a message, once; only the reviewed accountant replies.
/// Hiding a form is never the check (SDD authorization). Reads surface data failures as
/// <see cref="DataUnavailableException"/>; writes report <see cref="ReviewOutcome.Failed"/> or <see cref="ReplyOutcome.Failed"/>.
/// </summary>
public sealed class ReviewService(IReviewStore store, IClock clock)
{
    public const string StarsKey = nameof(ReviewForm.Stars);
    public const string TextKey = nameof(ReviewForm.Text);

    /// <summary>The review page for an accountant, or null when the profile isn't listed.</summary>
    public async Task<ReviewFormPage?> OpenFormAsync(ChatViewer? viewer, AccountantId profileId, CancellationToken cancellationToken = default)
    {
        if (await store.FindListedAsync(profileId, cancellationToken) is not { } target)
        {
            return null;
        }

        var access = await AccessAsync(viewer, target, cancellationToken);
        if (access != ReviewAccess.Allowed)
        {
            return new ReviewFormPage(target, access, null, null);
        }

        var mine = await store.FindAsync(profileId, viewer!.AccountId, cancellationToken);
        var name = ReviewerName.Short(await store.AccountNameAsync(viewer.AccountId, cancellationToken));
        return new ReviewFormPage(target, access, name, mine is null
            ? null
            : new ReviewForm { Stars = mine.Stars.ToString(CultureInfo.InvariantCulture), Text = mine.Text });
    }

    /// <summary>Posts the client's review, or updates it: there is one per client and accountant (BR09).</summary>
    public async Task<ReviewResult> SaveAsync(ChatViewer? viewer, AccountantId profileId, ReviewForm form, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await store.FindListedAsync(profileId, cancellationToken) is not { } target)
            {
                return new ReviewResult(ReviewOutcome.Gone);
            }

            var access = await AccessAsync(viewer, target, cancellationToken);
            if (access != ReviewAccess.Allowed)
            {
                return new ReviewResult(ReviewOutcome.NotAllowed, Access: access);
            }

            var errors = Validate(form, out var stars, out var text);
            if (errors.Count > 0)
            {
                return new ReviewResult(ReviewOutcome.Invalid, errors);
            }

            var updated = await store.SaveAsync(profileId, viewer!.AccountId, stars, text, clock.UtcNow, cancellationToken);
            return new ReviewResult(updated ? ReviewOutcome.Updated : ReviewOutcome.Posted);
        }
        catch (DataUnavailableException)
        {
            return new ReviewResult(ReviewOutcome.Failed);
        }
    }

    /// <summary>Deletes the client's own review and its reply (BR09). Nobody else can delete it (BR11).</summary>
    public async Task<ReviewResult> DeleteAsync(ChatViewer? viewer, AccountantId profileId, CancellationToken cancellationToken = default)
    {
        if (viewer is not { IsClient: true })
        {
            return new ReviewResult(ReviewOutcome.NotAllowed, Access: viewer is null ? ReviewAccess.SignIn : ReviewAccess.NotAClient);
        }

        try
        {
            if (await store.FindListedAsync(profileId, cancellationToken) is null)
            {
                return new ReviewResult(ReviewOutcome.Gone);
            }

            await store.DeleteAsync(profileId, viewer.AccountId, cancellationToken);
            return new ReviewResult(ReviewOutcome.Deleted);
        }
        catch (DataUnavailableException)
        {
            return new ReviewResult(ReviewOutcome.Failed);
        }
    }

    /// <summary>Posts or edits the accountant's one public reply to a review on their own profile (R14, BR11).</summary>
    public async Task<ReplyResult> ReplyAsync(ChatViewer? viewer, AccountantId profileId, Guid reviewId, string? text, CancellationToken cancellationToken = default)
    {
        if (viewer is null || viewer.IsClient)
        {
            return new ReplyResult(ReplyOutcome.NotAllowed);
        }

        try
        {
            if (await store.FindListedAsync(profileId, cancellationToken) is not { } target || target.OwnerAccountId != viewer.AccountId)
            {
                return new ReplyResult(ReplyOutcome.NotAllowed);
            }

            var normalized = ReviewText.Normalize(text);
            if (normalized is null)
            {
                return new ReplyResult(ReplyOutcome.Invalid, "Write a reply first.");
            }

            if (normalized.Length > ReviewLimits.Reply)
            {
                return new ReplyResult(ReplyOutcome.Invalid, string.Create(CultureInfo.InvariantCulture, $"Your reply can be at most {ReviewLimits.Reply} characters (now {normalized.Length:N0})."));
            }

            return await store.SaveReplyAsync(profileId, reviewId, normalized, clock.UtcNow, cancellationToken)
                ? new ReplyResult(ReplyOutcome.Posted)
                : new ReplyResult(ReplyOutcome.NotAllowed);
        }
        catch (DataUnavailableException)
        {
            return new ReplyResult(ReplyOutcome.Failed);
        }
    }

    /// <summary>The profile's rating and reviews for this viewer (R13). Empty for a profile that isn't listed.</summary>
    /// <exception cref="DataUnavailableException">The database could not be read.</exception>
    public async Task<ProfileReviews> ForProfileAsync(ChatViewer? viewer, AccountantId profileId, CancellationToken cancellationToken = default)
    {
        if (await store.FindListedAsync(profileId, cancellationToken) is not { } target)
        {
            return ProfileReviews.Empty;
        }

        var rows = await store.ListAsync(profileId, cancellationToken);
        var reviews = rows.Select(r => new ReviewView(
                r.Id,
                r.Stars,
                ReviewerName.Short(r.ClientName),
                r.Text,
                r.UpdatedAt,
                viewer is { IsClient: true } && r.ClientAccountId == viewer.AccountId,
                r.ReplyText,
                r.ReplyAt))
            .ToList();

        return new ProfileReviews(
            new RatingSummary(rows.Count, rows.Sum(r => r.Stars)),
            reviews,
            await AccessAsync(viewer, target, cancellationToken),
            reviews.Any(r => r.IsMine),
            viewer is { IsClient: false } && target.OwnerAccountId == viewer.AccountId);
    }

    /// <summary>BR09: a signed-in client whose conversation with this accountant has at least one message from the accountant.</summary>
    private async Task<ReviewAccess> AccessAsync(ChatViewer? viewer, ReviewTarget target, CancellationToken cancellationToken)
    {
        if (viewer is null)
        {
            return ReviewAccess.SignIn;
        }

        if (!viewer.IsClient)
        {
            return ReviewAccess.NotAClient;
        }

        return target.OwnerAccountId is { } accountant
               && await store.HasAccountantRepliedAsync(viewer.AccountId, accountant, cancellationToken)
            ? ReviewAccess.Allowed
            : ReviewAccess.NoReplyYet;
    }

    /// <summary>D23 (provisional): stars 1–5 required; text optional, no minimum, at most 1,000 characters. The prototype's words.</summary>
    public static Dictionary<string, string> Validate(ReviewForm form, out int stars, out string? text)
    {
        var errors = new Dictionary<string, string>();
        if (!int.TryParse(form.Stars, NumberStyles.None, CultureInfo.InvariantCulture, out stars)
            || stars is < ReviewLimits.MinStars or > ReviewLimits.MaxStars)
        {
            errors[StarsKey] = "Choose a rating from 1 to 5 stars.";
        }

        text = ReviewText.Normalize(form.Text);
        if (text?.Length > ReviewLimits.Text)
        {
            errors[TextKey] = string.Create(CultureInfo.InvariantCulture, $"Your review can be at most {ReviewLimits.Text:N0} characters (now {text.Length:N0}).");
        }

        return errors;
    }
}
