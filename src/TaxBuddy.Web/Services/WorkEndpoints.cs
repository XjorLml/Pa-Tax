using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using TaxBuddy.Web.Application.Chat;
using TaxBuddy.Web.Application.Work;
using TaxBuddy.Web.Components.Chat;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Services;

/// <summary>
/// F03 (R20): "Add accountant" and "Remove" post here from the profile and the workspace, then return to the page. A plain
/// form with antiforgery ([FromForm]), so it works on the static profile page without script (SDD render modes).
/// F03 stage 2 (R23): a service page's document upload posts here as a plain multipart form, and downloads come from here.
/// Endpoints of the one web app under the service's address, not a public API.
/// </summary>
public static class WorkEndpoints
{
    public const string SavedPath = "/my-accountants";

    /// <summary>The form's room besides the file: its token, key and task fields and the multipart framing.</summary>
    private const long FormOverhead = 64 * 1024;

    public static string FilesPath(Guid engagementId) => $"/work/{engagementId}/files";

    public static string FilePath(Guid engagementId, Guid fileId) => $"/work/{engagementId}/files/{fileId}";

    /// <summary>The service page's documents section, with an upload result and the task choice kept for a retry.</summary>
    public static string DocumentsAddress(Guid engagementId, string result, Guid? taskId = null) =>
        $"/work/{engagementId}?file={result}{(taskId is { } task ? $"&task={task}" : "")}#documents";

    public static IEndpointRouteBuilder MapWorkEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(SavedPath, async ([FromForm] string? profileId, [FromForm] string? on, [FromForm] string? returnUrl,
            HttpContext context, WorkService work) =>
        {
            if (!Guid.TryParse(profileId, out var id) || on is not ("true" or "false"))
            {
                return Results.BadRequest();
            }

            var outcome = await work.SaveAsync(ChatDisplay.ViewerOf(context.User), new AccountantId(id), on == "true");
            if (outcome == WorkOutcome.NotAllowed)
            {
                return Results.BadRequest();
            }

            var back = LocalAddress.OrHome(returnUrl);
            var result = outcome == WorkOutcome.Failed ? "failed" : on == "true" ? "added" : "removed";
            return Results.LocalRedirect(back + (back.Contains('?') ? "&" : "?") + "saved=" + result);
        });

        endpoints.MapPost("/work/{id:guid}/files", async (Guid id, HttpContext context, IAntiforgery antiforgery, ServiceDocuments documents) =>
        {
            // Refuse an oversized request before reading it (D26: 10 MB), then check the form's token.
            if (context.Features.Get<IHttpMaxRequestBodySizeFeature>() is { IsReadOnly: false } limit)
            {
                limit.MaxRequestBodySize = DocumentRules.MaxBytes + FormOverhead;
            }

            if (context.Request.ContentLength > DocumentRules.MaxBytes + FormOverhead)
            {
                return Results.LocalRedirect(DocumentsAddress(id, "too-large"));
            }

            if (!await IsValidAsync(antiforgery, context))
            {
                return Results.BadRequest();
            }

            var form = context.Request.Form;
            var taskId = Guid.TryParse(form["taskId"], out var task) ? task : (Guid?)null;
            if (!Guid.TryParse(form["key"], out var operationId))
            {
                return Results.BadRequest();
            }

            var file = form.Files["file"];
            if (file is null || file.Length == 0)
            {
                return Results.LocalRedirect(DocumentsAddress(id, "no-file", taskId));
            }

            if (file.Length > DocumentRules.MaxBytes)
            {
                return Results.LocalRedirect(DocumentsAddress(id, "too-large", taskId));
            }

            byte[] content;
            using (var buffer = new MemoryStream((int)file.Length))
            {
                await file.CopyToAsync(buffer, context.RequestAborted);
                content = buffer.ToArray();
            }

            var result = await documents.AttachAsync(ChatDisplay.ViewerOf(context.User), id, operationId, taskId, file.FileName, content, context.RequestAborted);
            return result switch
            {
                { Outcome: WorkOutcome.Done } => Results.LocalRedirect(DocumentsAddress(id, "attached")),
                { Outcome: WorkOutcome.NotAllowed } => Results.NotFound(),
                { Problem: var problem } => Results.LocalRedirect(DocumentsAddress(id, problem switch
                {
                    UploadProblem.NoFile => "no-file",
                    UploadProblem.TooLarge => "too-large",
                    UploadProblem.WrongType => "wrong-type",
                    UploadProblem.NoSuchTask => "no-task",
                    UploadProblem.Full => "full",
                    UploadProblem.Unavailable => "unavailable",
                    _ => "failed",
                }, problem == UploadProblem.NoSuchTask ? null : taskId)),
            };
        }).RequireAuthorization().DisableAntiforgery(); // validated by hand above, after the size limit is set

        // BR15: participants only; anyone else, or an unknown ID, gets the same 404. Always a download, never shown inline.
        endpoints.MapGet("/work/{id:guid}/files/{fileId:guid}", async (Guid id, Guid fileId, HttpContext context, ServiceDocuments documents) =>
        {
            var download = await documents.DownloadAsync(ChatDisplay.ViewerOf(context.User), id, fileId, context.RequestAborted);
            var headers = context.Response.Headers;
            headers.CacheControl = "private, no-store";
            switch (download.Outcome)
            {
                case DownloadOutcome.Ready:
                    headers.XContentTypeOptions = "nosniff";
                    headers.ContentSecurityPolicy = "default-src 'none'; sandbox";
                    return Results.File(download.Content!, download.ContentType, download.Name);
                case DownloadOutcome.Failed:
                    return Results.LocalRedirect(DocumentsAddress(id, "download-failed"));
                default:
                    return Results.NotFound();
            }
        }).RequireAuthorization();

        return endpoints;
    }

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
}
