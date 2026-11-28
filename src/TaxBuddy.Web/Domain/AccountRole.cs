namespace TaxBuddy.Web.Domain;

/// <summary>
/// What an account is for, chosen once at first sign-in (PRD R04). One role per account: PRD D03, decided by the
/// owner on 2026-11-16.
/// </summary>
public enum AccountRole
{
    Client = 1,
    Accountant = 2,
}

public static class AccountRoles
{
    /// <summary>The form value for a role; null for anything else, including an empty value.</summary>
    public static AccountRole? Parse(string? value) => value switch
    {
        "client" => AccountRole.Client,
        "accountant" => AccountRole.Accountant,
        _ => null,
    };

    public static string ValueOf(AccountRole role) => role switch
    {
        AccountRole.Client => "client",
        AccountRole.Accountant => "accountant",
        _ => throw new ArgumentOutOfRangeException(nameof(role)),
    };

    public static string Label(AccountRole role) => role switch
    {
        AccountRole.Client => "Client",
        AccountRole.Accountant => "Accountant",
        _ => throw new ArgumentOutOfRangeException(nameof(role)),
    };
}

/// <summary>Limits for account fields that come from the sign-in provider (SDD: abuse limits).</summary>
public static class AccountLimits
{
    public const int DisplayName = 100;
    public const int Email = 254;
    public const int TermsVersion = 40;
}

/// <summary>What is left of a deleted account (F12; PRD D07): the name the other person sees in their chats.</summary>
public static class DeletedAccount
{
    public const string DisplayName = "Deleted user";
}

/// <summary>
/// The terms and privacy text a person agrees to (PRD R16). Stored with the account; a person whose stored version
/// differs is asked to agree again (F11). The text lives in docs/legal and was approved by the owner on 2026-11-21
/// without a lawyer's review (D10). A change to the text needs a new version here and in both documents (F11 LP14).
/// Version 2026-11-23.2 adds saved accountants, service work and documents, and awaits the owner's approval.
/// </summary>
public static class LegalText
{
    public const string CurrentVersion = "2026-11-25";

    /// <summary>The date shown on the pages and the agree page.</summary>
    public const string UpdatedOn = "25 November 2026";

    /// <summary>The operator named in the text (owner's details of 2026-11-21; no registered business name yet).</summary>
    public const string Operator = "Lemnuel Lumaban";

    public const string OperatorPlace = "Manila, Metro Manila, Philippines";

    /// <summary>Support, privacy (Data Protection Officer) and takedown requests.</summary>
    public const string ContactEmail = "lumaban.lemnuel.1@gmail.com";
}
