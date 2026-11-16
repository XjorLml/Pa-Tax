using Microsoft.AspNetCore.Identity;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Data;

/// <summary>
/// A Tax Buddy account (PRD R04; ADR 006: ASP.NET Core Identity with external sign-ins only, so no password is ever
/// set). The email and name come from the sign-in provider; they are personal data and are never logged (SDD diagnostics).
/// </summary>
public sealed class TaxBuddyUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = "";

    public AccountRole Role { get; set; }

    /// <summary>When the person agreed to the terms and privacy policy (PRD R16), as a UTC instant (BR07).</summary>
    public DateTimeOffset AgreedToTermsAt { get; set; }

    /// <summary>Which text they agreed to: <see cref="LegalText.CurrentVersion"/> at the time.</summary>
    public string AgreedTermsVersion { get; set; } = "";

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// The current profile picture's object in Supabase Storage (F06; ADR 011), or null for the initials avatar.
    /// A new name for every upload, so a picture's address changes when the picture does.
    /// </summary>
    public string? PictureObject { get; set; }
}
