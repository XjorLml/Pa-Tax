using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Application.Reports;

/// <summary>Whether the viewer may report this item, and if not, why (DESIGN report states).</summary>
public enum ReportAccess
{
    Allowed,

    /// <summary>A visitor: reporting needs a signed-in user (SDD authorization).</summary>
    SignIn,

    /// <summary>Their own profile or review.</summary>
    OwnContent,

    /// <summary>One report per person per item (F10).</summary>
    AlreadyReported,
}

/// <summary>What is being reported, as the report page describes it, and where the reporter goes back to.</summary>
public sealed record ReportTarget(ReportKind Kind, Guid TargetId, AccountantId? ProfileId, string Description, string ReturnPath);

/// <summary>The report form as posted (a static form, F10). The reason arrives as text and is checked.</summary>
public sealed class ReportForm
{
    public string? Reason { get; set; }

    public string? Details { get; set; }
}

/// <summary>The report page: the item (null for a visitor asked to sign in) and whether the viewer may report it.</summary>
public sealed record ReportFormView(ReportTarget? Target, ReportAccess Access);

public enum ReportOutcome
{
    Sent,
    Invalid,
    AlreadyReported,

    /// <summary>More than <see cref="ReportLimits.PerDay"/> reports in a day.</summary>
    TooMany,
    NotAllowed,

    /// <summary>The item is gone or isn't visible to the reporter.</summary>
    Gone,
    Failed,
}

public sealed record ReportResult(ReportOutcome Outcome, IReadOnlyDictionary<string, string>? Errors = null, string? ReturnPath = null);

/// <summary>
/// The people and names behind a reported item, as the store finds them. For a profile, the owner is the accountant; for a
/// review, the client who wrote it; a conversation has both participants.
/// </summary>
public sealed record ReportSubject(
    AccountantId? ProfileId,
    string AccountantName,
    Guid? ContentOwnerAccountId,
    Guid? ClientAccountId,
    Guid? AccountantAccountId,
    string? ClientName);

/// <summary>Reports on PostgreSQL. Implemented in Data; failures surface as <see cref="Directory.DataUnavailableException"/>.</summary>
public interface IReportStore
{
    /// <summary>
    /// The item when it can still be reported: a listed profile, a review of a listed profile, or any conversation (the
    /// service checks that the reporter takes part). Null when it doesn't exist.
    /// </summary>
    Task<ReportSubject?> FindAsync(ReportKind kind, Guid targetId, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid reporterAccountId, ReportKind kind, Guid targetId, CancellationToken cancellationToken);

    Task<int> CountSinceAsync(Guid reporterAccountId, DateTimeOffset since, CancellationToken cancellationToken);

    /// <summary>Stores the report. False when this reporter already reported the item (the unique index decides).</summary>
    Task<bool> AddAsync(Report report, CancellationToken cancellationToken);
}
