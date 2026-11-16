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

/// <summary>
/// The terms and privacy text a person agrees to at registration (PRD R16). Stored with the account, so a later
/// version can ask people to agree again. The current text is a draft pending legal review (D10, gate G13).
/// </summary>
public static class LegalText
{
    public const string CurrentVersion = "draft-2026-11-16";
}
