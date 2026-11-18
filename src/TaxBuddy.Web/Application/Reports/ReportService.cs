using System.Globalization;
using Microsoft.Extensions.Logging;
using TaxBuddy.Web.Application.Chat;
using TaxBuddy.Web.Application.Directory;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Application.Reports;

/// <summary>
/// Reports to the site owner (PRD R15, BR12; F10). A signed-in user reports a listed profile or a review of one, or a
/// conversation they take part in, with a reason. Nobody reports their own profile or review. One report per person and
/// item, and <see cref="ReportLimits.PerDay"/> a day. The reported person is never told (BR12), and reported content stays
/// visible until the owner acts by hand (D22). Reports are read in the database; no names, IDs or text go to logs.
/// </summary>
public sealed class ReportService(IReportStore store, IClock clock, ILogger<ReportService> logger)
{
    public const string ReasonKey = nameof(ReportForm.Reason);
    public const string DetailsKey = nameof(ReportForm.Details);

    private static readonly EventId Received = new(1301, "ReportReceived");

    /// <summary>The report page for an item, or null when it doesn't exist or isn't visible to the viewer (a 404).</summary>
    /// <exception cref="DataUnavailableException">The database could not be read.</exception>
    public async Task<ReportFormView?> OpenAsync(ChatViewer? viewer, ReportKind kind, Guid targetId, CancellationToken cancellationToken = default)
    {
        if (viewer is null)
        {
            // A visitor learns nothing about a private conversation; profiles and reviews must still exist.
            return kind == ReportKind.Conversation || await store.FindAsync(kind, targetId, cancellationToken) is not null
                ? new ReportFormView(null, ReportAccess.SignIn)
                : null;
        }

        if (await TargetAsync(viewer, kind, targetId, cancellationToken) is not { } found)
        {
            return null;
        }

        var access = found.Own ? ReportAccess.OwnContent
            : await store.ExistsAsync(viewer.AccountId, kind, targetId, cancellationToken) ? ReportAccess.AlreadyReported
            : ReportAccess.Allowed;
        return new ReportFormView(found.Target, access);
    }

    /// <summary>Stores the report; the result carries the page to return to with the thanks line.</summary>
    public async Task<ReportResult> SendAsync(ChatViewer? viewer, ReportKind kind, Guid targetId, ReportForm form, CancellationToken cancellationToken = default)
    {
        if (viewer is null)
        {
            return new ReportResult(ReportOutcome.NotAllowed);
        }

        try
        {
            if (await TargetAsync(viewer, kind, targetId, cancellationToken) is not { } found)
            {
                return new ReportResult(ReportOutcome.Gone);
            }

            if (found.Own)
            {
                return new ReportResult(ReportOutcome.NotAllowed);
            }

            var errors = Validate(form, out var reason, out var details);
            if (errors.Count > 0)
            {
                return new ReportResult(ReportOutcome.Invalid, errors);
            }

            if (await store.ExistsAsync(viewer.AccountId, kind, targetId, cancellationToken))
            {
                return new ReportResult(ReportOutcome.AlreadyReported);
            }

            var now = clock.UtcNow;
            if (await store.CountSinceAsync(viewer.AccountId, now.AddDays(-1), cancellationToken) >= ReportLimits.PerDay)
            {
                return new ReportResult(ReportOutcome.TooMany);
            }

            if (!await store.AddAsync(new Report(Guid.NewGuid(), viewer.AccountId, kind, targetId, found.Target.ProfileId, reason, details, now), cancellationToken))
            {
                return new ReportResult(ReportOutcome.AlreadyReported);
            }

            // D22: the owner reads reports in the database. The log says only that one arrived, and of what kind.
            logger.LogInformation(Received, "Report received ({Kind})", kind);
            return new ReportResult(ReportOutcome.Sent, ReturnPath: ThanksPath(found.Target));
        }
        catch (DataUnavailableException)
        {
            return new ReportResult(ReportOutcome.Failed);
        }
    }

    /// <summary>A reason from the prototype's list is required; "Anything to add?" is optional, up to 600 characters.</summary>
    public static Dictionary<string, string> Validate(ReportForm form, out ReportReason reason, out string? details)
    {
        var errors = new Dictionary<string, string>();
        if (!Enum.TryParse(form.Reason, ignoreCase: false, out reason) || !Enum.IsDefined(reason) || int.TryParse(form.Reason, out _))
        {
            errors[ReasonKey] = "Choose a reason.";
        }

        details = ReviewText.Normalize(form.Details);
        if (details?.Length > ReportLimits.Details)
        {
            errors[DetailsKey] = string.Create(CultureInfo.InvariantCulture,
                $"What you add can be at most {ReportLimits.Details} characters (now {details.Length:N0}).");
        }

        return errors;
    }

    /// <summary>The item as the viewer may see it, and whether it is their own. Null when it is gone or not theirs to see.</summary>
    private async Task<(ReportTarget Target, bool Own)?> TargetAsync(ChatViewer viewer, ReportKind kind, Guid targetId, CancellationToken cancellationToken)
    {
        if (await store.FindAsync(kind, targetId, cancellationToken) is not { } subject)
        {
            return null;
        }

        switch (kind)
        {
            case ReportKind.Profile:
                return (new ReportTarget(kind, targetId, subject.ProfileId, $"{subject.AccountantName}'s profile",
                    $"/accountants/{subject.ProfileId}"), subject.ContentOwnerAccountId == viewer.AccountId);
            case ReportKind.Review:
                return (new ReportTarget(kind, targetId, subject.ProfileId,
                        $"{ReviewerName.Short(subject.ClientName)}'s review of {subject.AccountantName}",
                        $"/accountants/{subject.ProfileId}#review-{targetId:D}"),
                    subject.ContentOwnerAccountId == viewer.AccountId);
            default:
                // BR05: an outsider is told nothing, as if the conversation didn't exist.
                if (viewer.AccountId != subject.ClientAccountId && viewer.AccountId != subject.AccountantAccountId)
                {
                    return null;
                }

                var peer = viewer.AccountId == subject.ClientAccountId ? subject.AccountantName : subject.ClientName ?? "a client";
                return (new ReportTarget(kind, targetId, subject.ProfileId, $"Your conversation with {peer}", $"/chats/{targetId:D}"), false);
        }
    }

    /// <summary>Where a sent report goes back to, with the thanks line: the profile, the review or the chat.</summary>
    private static string ThanksPath(ReportTarget target) => target.Kind switch
    {
        ReportKind.Profile => $"/accountants/{target.ProfileId}?reported=profile",
        ReportKind.Review => $"/accountants/{target.ProfileId}?reported=review#review-{target.TargetId:D}",
        _ => $"/chats/{target.TargetId:D}?reported=1",
    };
}
