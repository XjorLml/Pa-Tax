namespace TaxBuddy.Web.Domain;

/// <summary>What is reported (PRD R15): a profile, a review or a conversation.</summary>
public enum ReportKind
{
    Profile,
    Review,
    Conversation,
}

/// <summary>Why it is reported: the prototype's reasons (<c>REPORT_REASONS</c>), adopted with F10 on 2026-11-18.</summary>
public enum ReportReason
{
    FakeOrMisleading,
    AbusiveOrHarassing,
    SpamOrScam,
    SharesPrivateInformation,
    SomethingElse,
}

/// <summary>Report limits (F10, the user's choice of 2026-11-18).</summary>
public static class ReportLimits
{
    /// <summary>The longest "Anything to add?" text, in characters after trimming (the prototype's 600).</summary>
    public const int Details = 600;

    /// <summary>Reports one account may send in a day. One per item is enforced separately.</summary>
    public const int PerDay = 10;

    public static IReadOnlyList<ReportReason> Reasons { get; } = Enum.GetValues<ReportReason>();

    /// <summary>The prototype's wording for each reason.</summary>
    public static string Text(ReportReason reason) => reason switch
    {
        ReportReason.FakeOrMisleading => "Fake or misleading",
        ReportReason.AbusiveOrHarassing => "Abusive or harassing",
        ReportReason.SpamOrScam => "Spam or a scam",
        ReportReason.SharesPrivateInformation => "Shares private information",
        _ => "Something else",
    };
}

/// <summary>
/// A report to the site owner (PRD R15, BR12). It needs a reason; the reported person is never told; the reported content
/// stays visible until the owner acts (D22: handled by hand, read from the database). One per reporter and item.
/// </summary>
public sealed class Report
{
    private Report()
    {
    }

    public Report(Guid id, Guid reporterAccountId, ReportKind kind, Guid targetId, AccountantId? profileId, ReportReason reason,
        string? details, DateTimeOffset createdAt)
    {
        if (!Enum.IsDefined(reason))
        {
            throw new ArgumentOutOfRangeException(nameof(reason));
        }

        var normalized = ReviewText.Normalize(details);
        if (normalized?.Length > ReportLimits.Details)
        {
            throw new ArgumentException($"Details can be at most {ReportLimits.Details} characters.", nameof(details));
        }

        Id = id;
        ReporterAccountId = reporterAccountId;
        Kind = kind;
        TargetId = targetId;
        ProfileId = profileId;
        Reason = reason;
        Details = normalized;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public Guid ReporterAccountId { get; private set; }

    public ReportKind Kind { get; private set; }

    /// <summary>The profile, review or conversation ID.</summary>
    public Guid TargetId { get; private set; }

    /// <summary>The accountant profile the item belongs to, when there is one: for finding it in the database.</summary>
    public AccountantId? ProfileId { get; private set; }

    public ReportReason Reason { get; private set; }

    public string? Details { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
}
