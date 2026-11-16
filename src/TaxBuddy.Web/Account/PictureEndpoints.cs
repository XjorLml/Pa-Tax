using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using TaxBuddy.Web.Application.Pictures;
using TaxBuddy.Web.Data;

namespace TaxBuddy.Web.Account;

/// <summary>
/// Profile pictures (F06; ADR 011): upload and remove from the Account page's plain forms, and serve pictures with the
/// access rules applied. Endpoints of the one web app, not a public API.
/// </summary>
public static class PictureEndpoints
{
    public const string UploadPath = "/account/picture";
    public const string RemovePath = "/account/picture/remove";

    /// <summary>The Account page's picture section, where results are shown.</summary>
    public static string AccountAddress(string result) => $"/account?picture={result}#picture";

    public static IEndpointRouteBuilder MapPictureEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(UploadPath, async (HttpContext context, IAntiforgery antiforgery, ProfilePictureService pictures,
            UserManager<TaxBuddyUser> users, SignInManager<TaxBuddyUser> signIn) =>
        {
            // Refuse oversized requests before reading them: the file plus room for the form's other fields.
            if (context.Features.Get<IHttpMaxRequestBodySizeFeature>() is { IsReadOnly: false } limit)
            {
                limit.MaxRequestBodySize = PictureLimits.MaxUploadBytes + 64 * 1024;
            }

            if (context.Request.ContentLength > PictureLimits.MaxUploadBytes + 64 * 1024)
            {
                return Results.LocalRedirect(AccountAddress("too-large"));
            }

            if (!await IsValidAsync(antiforgery, context))
            {
                return Results.BadRequest();
            }

            if (Owner(context) is not { } owner)
            {
                return Results.Forbid();
            }

            var file = context.Request.Form.Files["picture"];
            if (file is null || file.Length == 0)
            {
                return Results.LocalRedirect(AccountAddress("no-file"));
            }

            byte[] upload;
            using (var buffer = new MemoryStream((int)Math.Min(file.Length, PictureLimits.MaxUploadBytes + 1)))
            {
                await file.CopyToAsync(buffer, context.RequestAborted);
                upload = buffer.ToArray();
            }

            var result = await pictures.UploadAsync(owner, upload, context.RequestAborted);
            if (result.Outcome == PictureUploadOutcome.Saved)
            {
                await RefreshSessionAsync(context, users, signIn);
            }

            return Results.LocalRedirect(AccountAddress(result switch
            {
                { Outcome: PictureUploadOutcome.Saved } => "saved",
                { Problem: PictureProblem.TooLarge } => "too-large",
                { Problem: PictureProblem.TooManyPixels } => "too-many-pixels",
                { Problem: PictureProblem.NoFile } => "no-file",
                { Outcome: PictureUploadOutcome.Rejected } => "not-an-image",
                { Outcome: PictureUploadOutcome.Unavailable } => "unavailable",
                _ => "failed",
            }));
        }).RequireAuthorization().DisableAntiforgery(); // validated by hand above, after the size limit is set

        endpoints.MapPost(RemovePath, async (HttpContext context, IAntiforgery antiforgery, ProfilePictureService pictures,
            UserManager<TaxBuddyUser> users, SignInManager<TaxBuddyUser> signIn) =>
        {
            if (!await IsValidAsync(antiforgery, context))
            {
                return Results.BadRequest();
            }

            if (Owner(context) is not { } owner)
            {
                return Results.Forbid();
            }

            if (!await pictures.RemoveAsync(owner, context.RequestAborted))
            {
                return Results.LocalRedirect(AccountAddress("failed"));
            }

            await RefreshSessionAsync(context, users, signIn);
            return Results.LocalRedirect(AccountAddress("removed"));
        }).RequireAuthorization().DisableAntiforgery();

        // An accountant's picture is public; a client's goes to that client only; anything else is 404 (PP05).
        endpoints.MapGet("/pictures/{accountId:guid}", async (Guid accountId, string? v, HttpContext context, ProfilePictureService pictures) =>
        {
            if (await pictures.ReadAsync(accountId, Owner(context), context.RequestAborted) is not { } picture)
            {
                return Results.NotFound();
            }

            var headers = context.Response.Headers;
            headers.CacheControl = !picture.IsPublic ? "private, max-age=3600"
                : v == picture.Version ? "public, max-age=31536000, immutable"
                : "public, max-age=300";
            headers.ETag = $"\"{picture.Version}\"";
            headers.XContentTypeOptions = "nosniff";
            return Results.File(picture.Jpeg, "image/jpeg");
        });

        return endpoints;
    }

    private static Guid? Owner(HttpContext context) =>
        Guid.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    private static async Task<bool> IsValidAsync(IAntiforgery antiforgery, HttpContext context)
    {
        try
        {
            await antiforgery.ValidateRequestAsync(context);
            return true;
        }
        catch (AntiforgeryValidationException)
        {
            return false;
        }
        catch (BadHttpRequestException)
        {
            return false; // the body was over the limit while the form was read
        }
    }

    /// <summary>Re-issues the session cookie, so the header's picture claim matches straight away.</summary>
    private static async Task RefreshSessionAsync(HttpContext context, UserManager<TaxBuddyUser> users, SignInManager<TaxBuddyUser> signIn)
    {
        if (await users.GetUserAsync(context.User) is { } user)
        {
            await signIn.RefreshSignInAsync(user);
        }
    }
}
