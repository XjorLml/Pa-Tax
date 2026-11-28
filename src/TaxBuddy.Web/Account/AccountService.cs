using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaxBuddy.Web.Application.Chat;
using TaxBuddy.Web.Application.Pictures;
using TaxBuddy.Web.Application.Work;
using TaxBuddy.Web.Data;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Account;

/// <summary>A first sign-in waiting for registration, as the registration page shows it.</summary>
public sealed record PendingSignIn(string ProviderName, string? Email, string DisplayName);

public enum RegistrationOutcome
{
    SignedIn,
    NoPendingSignIn,
    Failed,
}

/// <summary>The outcome, and when signed in, the role the account actually has (another tab may have registered it first).</summary>
public sealed record RegistrationResult(RegistrationOutcome Outcome, AccountRole? Role = null);

public enum AgreementOutcome
{
    Agreed,
    NotSignedIn,
    Failed,
}

public enum DeletionOutcome
{
    Deleted,
    NotSignedIn,

    /// <summary>The database couldn't be reached; nothing was changed and the person is still signed in.</summary>
    Failed,
}

/// <summary>
/// What the sign-in, registration and account pages need (F04, F11, F12). Pages use this rather than Identity's managers,
/// so components never depend on the data layer (SDD dependency rules).
/// </summary>
public sealed class AccountService(
    UserManager<TaxBuddyUser> users,
    SignInManager<TaxBuddyUser> signIn,
    IAuthenticationSchemeProvider schemes,
    IDbContextFactory<TaxBuddyDbContext> contexts,
    IPictureStorage pictures,
    IDocumentStorage documents,
    IChatNotifier chatNotifier,
    IClock clock,
    ILogger<AccountService> logger)
{
    private static readonly EventId AccountDeleted = new(1401, "AccountDeleted");
    private static readonly EventId DeletionFailed = new(1402, "AccountDeletionFailed");
    private static readonly EventId PictureLeftInStorage = new(1403, "DeletedAccountPictureLeft");
    private static readonly EventId DocumentsLeftInStorage = new(1404, "DeletedAccountDocumentsLeft");

    public async Task<bool> IsGoogleAvailableAsync() =>
        await schemes.GetSchemeAsync(AccountSetup.GoogleScheme) is not null;

    public async Task<bool> IsMicrosoftAvailableAsync() =>
        await schemes.GetSchemeAsync(AccountSetup.MicrosoftScheme) is not null;

    /// <summary>The sign-in choices this site offers, for wording: "Google", "Microsoft" or "Google or Microsoft" (F15).</summary>
    public async Task<string> ProviderNamesAsync()
    {
        var google = await IsGoogleAvailableAsync();
        var microsoft = await IsMicrosoftAvailableAsync();
        return google && microsoft ? "Google or Microsoft" : microsoft ? "Microsoft" : "Google";
    }

    public async Task<PendingSignIn?> GetPendingAsync() =>
        await signIn.GetExternalLoginInfoAsync() is { } info
            ? new PendingSignIn(info.ProviderDisplayName ?? info.LoginProvider, EmailOf(info), DisplayNameOf(info))
            : null;

    /// <summary>
    /// Creates the account with the one chosen role (D03) and the agreement time (R16), links the provider sign-in and
    /// signs in. A repeated submit after success signs in to the account the first one created.
    /// </summary>
    public async Task<RegistrationResult> RegisterAsync(AccountRole role)
    {
        if (await signIn.GetExternalLoginInfoAsync() is not { } info)
        {
            return new RegistrationResult(RegistrationOutcome.NoPendingSignIn);
        }

        if (await users.FindByLoginAsync(info.LoginProvider, info.ProviderKey) is { } existing)
        {
            return await SignInAsync(existing, info);
        }

        var now = clock.UtcNow;
        var id = Guid.CreateVersion7();
        var user = new TaxBuddyUser
        {
            Id = id,
            UserName = id.ToString("N"),
            Email = EmailOf(info),
            DisplayName = DisplayNameOf(info),
            Role = role,
            AgreedToTermsAt = now,
            AgreedTermsVersion = LegalText.CurrentVersion,
            CreatedAt = now,
        };

        var created = await users.CreateAsync(user);
        if (!created.Succeeded)
        {
            // SDD diagnostics: Identity's error codes only, never the email or name.
            logger.LogWarning("Account creation failed: {Codes}", string.Join(",", created.Errors.Select(e => e.Code)));
            return new RegistrationResult(RegistrationOutcome.Failed);
        }

        try
        {
            var linked = await users.AddLoginAsync(user, new UserLoginInfo(info.LoginProvider, info.ProviderKey, info.ProviderDisplayName));
            if (!linked.Succeeded)
            {
                logger.LogWarning("Linking the sign-in failed: {Codes}", string.Join(",", linked.Errors.Select(e => e.Code)));
                await users.DeleteAsync(user);
                return new RegistrationResult(RegistrationOutcome.Failed);
            }
        }
        catch (DbUpdateException)
        {
            // Two submits at once: the other request linked this sign-in first. Keep its account.
            await users.DeleteAsync(user);
            return await users.FindByLoginAsync(info.LoginProvider, info.ProviderKey) is { } winner
                ? await SignInAsync(winner, info)
                : new RegistrationResult(RegistrationOutcome.Failed);
        }

        return await SignInAsync(user, info);
    }

    /// <summary>
    /// F11 LP06: records agreement to the current terms for the signed-in account, then renews the session so its
    /// version claim matches. An account already at the current version (agreed in another browser) is only renewed.
    /// </summary>
    public async Task<AgreementOutcome> AgreeToCurrentAsync()
    {
        if (await users.GetUserAsync(signIn.Context.User) is not { } user)
        {
            return AgreementOutcome.NotSignedIn;
        }

        if (user.AgreedTermsVersion != LegalText.CurrentVersion)
        {
            user.AgreedTermsVersion = LegalText.CurrentVersion;
            user.AgreedToTermsAt = clock.UtcNow;
            var updated = await users.UpdateAsync(user);
            if (!updated.Succeeded)
            {
                logger.LogWarning("Saving agreement failed: {Codes}", string.Join(",", updated.Errors.Select(e => e.Code)));
                return AgreementOutcome.Failed;
            }
        }

        await signIn.RefreshSignInAsync(user);
        return AgreementOutcome.Agreed;
    }

    /// <summary>
    /// F12 (PRD R09, BR06, D07): deletes the signed-in account in one transaction, then its picture, then signs out.
    /// <list type="bullet">
    /// <item>An accountant's profile goes, and with it its services, experience, highlights, reviews and replies.</item>
    /// <item>A client's reviews go; ratings are computed, so they recalculate.</item>
    /// <item>Each conversation stays with the other person, closed by this side's block. One whose other person was already
    /// deleted goes, with its messages.</item>
    /// <item>Service documents the account uploaded go (D27, partly), each service noting it in its history; the rest of the
    /// service work stays with the other person.</item>
    /// <item>The account row stays, nameless, without email, picture or sign-in links, marked with the deletion time, because
    /// the other person's chats point at it. A new security stamp ends its other sessions at the next check.</item>
    /// </list>
    /// </summary>
    public async Task<DeletionOutcome> DeleteAccountAsync(CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(users.GetUserId(signIn.Context.User), out var id))
        {
            return DeletionOutcome.NotSignedIn;
        }

        string? picture;
        List<string> documentObjects;
        List<Conversation> changed;
        try
        {
            await using var db = await contexts.CreateDbContextAsync(cancellationToken);
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null, cancellationToken);
            if (user is null)
            {
                await signIn.SignOutAsync();
                return DeletionOutcome.NotSignedIn;
            }

            var now = clock.UtcNow;
            picture = user.PictureObject;

            await db.AccountantProfiles.Where(p => p.OwnerAccountId == id).ExecuteDeleteAsync(cancellationToken);
            await db.Reviews.Where(r => r.ClientAccountId == id).ExecuteDeleteAsync(cancellationToken);
            // F03: the private saved list goes; shared service work stays with the other person, like chats (D27 open for the rest).
            await db.SavedAccountants.Where(s => s.ClientAccountId == id).ExecuteDeleteAsync(cancellationToken);

            // F03 stage 2 (D27, partly): the documents this account uploaded go; their bytes go after the commit.
            var uploaded = await db.ServiceFiles.Where(f => f.UploaderAccountId == id)
                .Select(f => new { f.EngagementId, f.ObjectName }).ToListAsync(cancellationToken);
            documentObjects = uploaded.Select(f => f.ObjectName).ToList();
            foreach (var group in uploaded.GroupBy(f => f.EngagementId))
            {
                var count = group.Count();
                db.ServiceUpdates.Add(new ServiceUpdate(Guid.NewGuid(), group.Key, id,
                    $"This account was deleted, so the {(count == 1 ? "document" : $"{count} documents")} it added {(count == 1 ? "was" : "were")} removed.", false, now));
            }

            await db.ServiceFiles.Where(f => f.UploaderAccountId == id).ExecuteDeleteAsync(cancellationToken);

            var conversations = await db.Conversations
                .Where(c => c.ClientAccountId == id || c.AccountantAccountId == id)
                .ToListAsync(cancellationToken);
            var peers = conversations.Select(c => c.OtherThan(id)).Distinct().ToList();
            var deletedPeers = await db.Users.Where(u => peers.Contains(u.Id) && u.DeletedAt != null)
                .Select(u => u.Id).ToListAsync(cancellationToken);
            foreach (var conversation in conversations)
            {
                if (deletedPeers.Contains(conversation.OtherThan(id)))
                {
                    db.Conversations.Remove(conversation); // Nobody is left to read it; messages go with it.
                }
                else
                {
                    conversation.Block(id, now); // The other person can read it but no longer reply (D07).
                }
            }

            // F14: no more emails; pending ones go now (sent ones are removed by the outbox after 30 days).
            await db.EmailJobs.Where(j => j.RecipientAccountId == id && j.Status == EmailJobStatus.Pending).ExecuteDeleteAsync(cancellationToken);

            await db.UserLogins.Where(l => l.UserId == id).ExecuteDeleteAsync(cancellationToken);
            await db.UserClaims.Where(c => c.UserId == id).ExecuteDeleteAsync(cancellationToken);
            await db.UserTokens.Where(t => t.UserId == id).ExecuteDeleteAsync(cancellationToken);

            user.DisplayName = DeletedAccount.DisplayName;
            user.Email = null;
            user.NormalizedEmail = null;
            user.PictureObject = null;
            user.EmailNotifications = false;
            // F17 (BR06): a client's details go with the name and email.
            user.PhoneNumber = null;
            user.City = null;
            user.BusinessName = null;
            user.BusinessType = null;
            user.Industry = null;
            user.DeletedAt = now;
            user.LockoutEnd = DateTimeOffset.MaxValue;
            user.SecurityStamp = Guid.NewGuid().ToString("N");
            user.ConcurrencyStamp = Guid.NewGuid().ToString();

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            changed = conversations.Where(c => !deletedPeers.Contains(c.OtherThan(id))).ToList();
        }
        catch (Exception ex) when (ex is System.Data.Common.DbException or DbUpdateException or TimeoutException
                                       or InvalidOperationException { InnerException: System.Data.Common.DbException or TimeoutException })
        {
            // SDD diagnostics: the error category only, never the account's ID, name or email.
            logger.LogWarning(DeletionFailed, "Account deletion failed ({ErrorType})", ex.GetType().Name);
            return DeletionOutcome.Failed;
        }

        foreach (var conversation in changed)
        {
            chatNotifier.Changed(conversation); // The other person's open chat shows the deleted note at once.
        }

        if (picture is not null)
        {
            try
            {
                await pictures.DeleteAsync(picture, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // The account is already deleted; the object is unreachable through the site. RELEASE: clean up by hand.
                logger.LogWarning(PictureLeftInStorage, "A deleted account's picture stayed in storage ({ErrorType})", ex.GetType().Name);
            }
        }

        var documentsLeft = 0;
        foreach (var objectName in documentObjects)
        {
            try
            {
                await documents.DeleteAsync(objectName, CancellationToken.None);
            }
            catch (DocumentStorageException)
            {
                documentsLeft++;
            }
        }

        if (documentsLeft > 0)
        {
            // As for the picture: unreachable through the site, cleaned up by hand (RELEASE).
            logger.LogWarning(DocumentsLeftInStorage, "{Count} of a deleted account's documents stayed in storage", documentsLeft);
        }

        logger.LogInformation(AccountDeleted, "Account deleted");
        await signIn.SignOutAsync();
        return DeletionOutcome.Deleted;
    }

    /// <summary>True when the account in the database already has the current version, though the session doesn't.</summary>
    public async Task<bool> HasAgreedToCurrentAsync() =>
        await users.GetUserAsync(signIn.Context.User) is { } user && user.AgreedTermsVersion == LegalText.CurrentVersion;

    private async Task<RegistrationResult> SignInAsync(TaxBuddyUser user, ExternalLoginInfo info)
    {
        await signIn.SignInAsync(user, isPersistent: true, info.LoginProvider);
        await signIn.Context.SignOutAsync(IdentityConstants.ExternalScheme);
        return new RegistrationResult(RegistrationOutcome.SignedIn, user.Role);
    }

    private static string? EmailOf(ExternalLoginInfo info) =>
        Limit(info.Principal.FindFirstValue(ClaimTypes.Email), AccountLimits.Email);

    private static string DisplayNameOf(ExternalLoginInfo info) =>
        Limit(info.Principal.FindFirstValue(ClaimTypes.Name), AccountLimits.DisplayName)
        ?? Limit(info.Principal.FindFirstValue(ClaimTypes.GivenName), AccountLimits.DisplayName)
        ?? "Tax Buddy user";

    private static string? Limit(string? value, int length)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed.Length <= length ? trimmed : trimmed[..length];
    }
}
