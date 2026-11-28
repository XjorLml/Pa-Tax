using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TaxBuddy.TestSupport;
using TaxBuddy.Web.Domain;
using TaxBuddy.Web.Services;
using TaxBuddy.Web.Tests.Support;
using static TaxBuddy.Web.Tests.Support.FormPosts;

namespace TaxBuddy.Web.Tests;

/// <summary>
/// F03 stage 2 through the whole app (CS06–CS08; PRD R23, BR15, BR17, BR18; D26–D28): real uploads checked by content,
/// downloads for the two participants only, uploader-only removal, retries that store one document, storage failures,
/// and documents leaving with their uploader's account.
/// </summary>
public sealed class ServiceDocumentsTests(EmptyAppFixture app) : IClassFixture<EmptyAppFixture>
{
    private static readonly byte[] SamplePdf = Encoding.ASCII.GetBytes("%PDF-1.4\n1 0 obj\n<< /Type /Catalog >>\nendobj\ntrailer\n<< /Root 1 0 R >>\n%%EOF\n");

    [DatabaseFact]
    public async Task A_client_attaches_a_pdf_to_a_task_and_the_accountant_downloads_it()
    {
        var (accountant, client, service) = await AcceptedAsync("Bookkeeping");
        var task = await TaskIdAsync(service, 1);

        var uploaded = await client.Client.PostAsync(WorkEndpoints.FilesPath(service), await UploadFormAsync(client, service, SamplePdf, "March receipts.pdf", task));
        Assert.Equal($"/work/{service}?file=attached#documents", Target(uploaded));

        var page = await accountant.Client.GetHtmlAsync($"/work/{service}");
        Assert.Contains("Private to Olive Reyes and Nina Ocampo.", page);
        Assert.Contains("<strong>March receipts.pdf</strong>", page);
        Assert.Contains("Available · Olive Reyes · ", page);
        Assert.Contains("· Prepare the work · 1 KB", page);
        Assert.Contains("Attached March receipts.pdf to Prepare the work.", page);
        Assert.DoesNotContain("Remove<span class=\"sr-only\"> March receipts.pdf", page); // the counterpart can't remove it

        var file = await SingleFileAsync(service);
        Assert.Equal(DocumentRules.Pdf, file.ContentType);
        Assert.Equal($"services/{service:N}/{file.Id:N}", file.ObjectName);
        Assert.Equal(SamplePdf, app.Factory.Documents.Objects[file.ObjectName].Content);

        var download = await accountant.Client.GetAsync(WorkEndpoints.FilePath(service, file.Id));
        Assert.Equal(HttpStatusCode.OK, download.StatusCode);
        Assert.Equal(SamplePdf, await download.Content.ReadAsByteArrayAsync());
        Assert.Equal("application/pdf", download.Content.Headers.ContentType!.MediaType);
        Assert.Equal("attachment", download.Content.Headers.ContentDisposition!.DispositionType);
        Assert.Equal("March receipts.pdf", download.Content.Headers.ContentDisposition.FileNameStar);
        Assert.Equal("nosniff", download.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Contains("no-store", download.Headers.CacheControl!.ToString());

        Assert.Contains("1 service document", await client.Client.GetHtmlAsync("/workspace"));
        Assert.Contains("1 service document", await accountant.Client.GetHtmlAsync($"/clients/{client.Id}"));
    }

    [DatabaseFact]
    public async Task Only_real_pdf_jpeg_and_png_files_up_to_10_MB_are_kept_and_the_task_choice_survives()
    {
        var (_, client, service) = await AcceptedAsync("Payroll");
        var task = await TaskIdAsync(service, 2);

        var cases = new (byte[] Content, string Name, string Result)[]
        {
            (Encoding.ASCII.GetBytes("MZ this is a program"), "invoice.pdf", "wrong-type"),
            ([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3, 4], "photo.png", "wrong-type"), // a PNG signature, no image
            (Encoding.ASCII.GetBytes("%PDF-1.4 cut short"), "short.pdf", "wrong-type"), // no end marker
            (new byte[DocumentRules.MaxBytes + 1], "big.pdf", "too-large"),
            ([], "empty.pdf", "no-file"),
        };
        foreach (var (content, name, result) in cases)
        {
            var response = await client.Client.PostAsync(WorkEndpoints.FilesPath(service), await UploadFormAsync(client, service, content, name, task));
            Assert.Equal($"/work/{service}?file={result}&task={task}#documents", Target(response));
        }

        var retry = await client.Client.GetHtmlAsync($"/work/{service}?file=wrong-type&task={task}");
        Assert.Contains("That file isn't a PDF, JPEG or PNG. Choose one of those.", retry);
        Assert.Matches($"""<option value="{task}" selected""", retry);
        Assert.Contains(">Retry upload</button>", retry);

        var (_, _, other) = await AcceptedAsync("Audit");
        var foreignTask = await TaskIdAsync(other, 0);
        Assert.Equal($"/work/{service}?file=no-task#documents",
            Target(await client.Client.PostAsync(WorkEndpoints.FilesPath(service), await UploadFormAsync(client, service, SamplePdf, "a.pdf", foreignTask))));

        await using var db = app.Schema.CreateContext();
        Assert.False(await db.ServiceFiles.AnyAsync(f => f.EngagementId == service));
        Assert.DoesNotContain(app.Factory.Documents.Objects.Keys, k => k.StartsWith($"services/{service:N}/", StringComparison.Ordinal));

        // A JPEG and a PNG are kept; a name with path or markup characters is made safe and ends in the content's extension.
        await client.Client.PostAsync(WorkEndpoints.FilesPath(service), await UploadFormAsync(client, service, TestImages.Jpeg(), "scan.jpeg"));
        await client.Client.PostAsync(WorkEndpoints.FilesPath(service), await UploadFormAsync(client, service, TestImages.Png(40, 20), @"..\..\<i>notes.pdf"));
        var names = await db.ServiceFiles.Where(f => f.EngagementId == service).OrderBy(f => f.CreatedAt).Select(f => new { f.DisplayName, f.ContentType }).ToListAsync();
        Assert.Equal(new[] { ("scan.jpeg", DocumentRules.Jpeg), ("inotes.pdf.png", DocumentRules.Png) }, names.Select(n => (n.DisplayName, n.ContentType)).ToArray());
    }

    [DatabaseFact]
    public async Task Sending_the_same_upload_form_twice_stores_one_document()
    {
        var (_, client, service) = await AcceptedAsync("Tax filing");
        var page = await client.Client.GetHtmlAsync($"/work/{service}");

        for (var i = 0; i < 2; i++)
        {
            var response = await client.Client.PostAsync(WorkEndpoints.FilesPath(service), UploadForm(page, SamplePdf, "return.pdf"));
            Assert.Equal($"/work/{service}?file=attached#documents", Target(response));
        }

        await using var db = app.Schema.CreateContext();
        Assert.Equal(1, await db.ServiceFiles.CountAsync(f => f.EngagementId == service));
        Assert.Single(app.Factory.Documents.Objects.Keys, k => k.StartsWith($"services/{service:N}/", StringComparison.Ordinal));
        Assert.Equal(1, await db.ServiceUpdates.CountAsync(u => u.EngagementId == service && u.Text == "Attached return.pdf to the service."));
    }

    [DatabaseFact]
    public async Task An_older_copy_of_the_upload_form_with_another_file_or_task_stores_a_new_document()
    {
        // PRD D31: after the browser's Back the form can return with its key already used. Another file, or the same file
        // for another task, is a new document; only the same file and task again is a repeat. It said "Document attached."
        // and stored nothing.
        var (_, client, service) = await AcceptedAsync("Payroll");
        var task = await TaskIdAsync(service, 1);
        var page = await client.Client.GetHtmlAsync($"/work/{service}");
        var otherPdf = SamplePdf.Concat("% another file\n"u8.ToArray()).ToArray();

        foreach (var (content, name, forTask) in new[]
        {
            (SamplePdf, "return.pdf", (Guid?)null), (otherPdf, "receipt.pdf", null), (SamplePdf, "return.pdf", task), (SamplePdf, "return.pdf", null),
        })
        {
            var response = await client.Client.PostAsync(WorkEndpoints.FilesPath(service), UploadForm(page, content, name, forTask));
            Assert.Equal($"/work/{service}?file=attached#documents", Target(response));
        }

        await using var db = app.Schema.CreateContext();
        var files = await db.ServiceFiles.Where(f => f.EngagementId == service).Select(f => new { f.DisplayName, f.TaskId }).ToListAsync();
        Assert.Equal(3, files.Count);
        Assert.Single(files, f => f.DisplayName == "receipt.pdf");
        Assert.Single(files, f => f.DisplayName == "return.pdf" && f.TaskId == task);
    }

    [DatabaseFact]
    public async Task Nobody_else_downloads_or_adds_documents_and_requests_take_none()
    {
        var (accountant, client, service) = await AcceptedAsync("Business registration");
        await client.Client.PostAsync(WorkEndpoints.FilesPath(service), await UploadFormAsync(client, service, SamplePdf, "permit.pdf"));
        var file = await SingleFileAsync(service);
        var stranger = await RegisterAsync(AccountRole.Client, "Rina Cruz");
        var otherAccountant = await RegisterAsync(AccountRole.Accountant, "Paolo Lim");

        foreach (var other in new[] { stranger, otherAccountant })
        {
            var download = await other.Client.GetAsync(WorkEndpoints.FilePath(service, file.Id));
            Assert.Equal(HttpStatusCode.NotFound, download.StatusCode);
            Assert.DoesNotContain("permit", await download.Content.ReadAsStringAsync());

            // Their own page's token is valid for them; the service still isn't theirs.
            var ownPage = await other.Client.GetHtmlAsync("/workspace");
            var forged = new MultipartFormDataContent
            {
                { new StringContent(Token(ownPage)), "__RequestVerificationToken" },
                { new StringContent(Guid.NewGuid().ToString()), "key" },
                { new ByteArrayContent(SamplePdf), "file", "forged.pdf" },
            };
            Assert.Equal(HttpStatusCode.NotFound, (await other.Client.PostAsync(WorkEndpoints.FilesPath(service), forged)).StatusCode);
        }

        // A guessed file ID under the right service, and the right file under another service, are both unknown.
        Assert.Equal(HttpStatusCode.NotFound, (await client.Client.GetAsync(WorkEndpoints.FilePath(service, Guid.NewGuid()))).StatusCode);
        var (_, _, otherService) = await AcceptedAsync("Audit");
        Assert.Equal(HttpStatusCode.NotFound, (await accountant.Client.GetAsync(WorkEndpoints.FilePath(otherService, file.Id))).StatusCode);

        var visitor = await app.Factory.CreateHttpsClient().GetAsync(WorkEndpoints.FilePath(service, file.Id));
        Assert.Equal(HttpStatusCode.Redirect, visitor.StatusCode);
        Assert.StartsWith("/signin", Target(visitor));

        var (_, requestedClient, requested) = await RequestAsync("Payroll");
        var requestPage = await requestedClient.Client.GetHtmlAsync($"/work/{requested}");
        Assert.Contains("Documents can be added after acceptance.", requestPage);
        Assert.DoesNotContain("Upload document", requestPage);
        var early = new MultipartFormDataContent
        {
            { new StringContent(Token(requestPage)), "__RequestVerificationToken" },
            { new StringContent(Guid.NewGuid().ToString()), "key" },
            { new ByteArrayContent(SamplePdf), "file", "early.pdf" },
        };
        Assert.Equal(HttpStatusCode.NotFound, (await requestedClient.Client.PostAsync(WorkEndpoints.FilesPath(requested), early)).StatusCode);

        await using var db = app.Schema.CreateContext();
        Assert.Equal(1, await db.ServiceFiles.CountAsync(f => f.EngagementId == service));
        Assert.False(await db.ServiceFiles.AnyAsync(f => f.EngagementId == requested));
    }

    [DatabaseFact]
    public async Task Only_the_uploader_removes_a_document_after_confirming()
    {
        var (accountant, client, service) = await AcceptedAsync("Bookkeeping");
        await accountant.Client.PostAsync(WorkEndpoints.FilesPath(service), await UploadFormAsync(accountant, service, TestImages.Png(40, 20), "checklist.png"));
        var file = await SingleFileAsync(service);

        var own = await accountant.Client.GetHtmlAsync($"/work/{service}");
        Assert.Contains($"""href="/work/{service}?remove={file.Id}#documents">Remove<span class="sr-only"> checklist.png</span></a>""", own);
        Assert.DoesNotContain(">Remove<span class=\"sr-only\"> checklist.png", await client.Client.GetHtmlAsync($"/work/{service}"));

        // The client can't remove it: the confirm step isn't drawn for them, and a forged post changes nothing.
        var clientConfirm = await client.Client.GetHtmlAsync($"/work/{service}?remove={file.Id}");
        Assert.DoesNotContain("Remove checklist.png?", clientConfirm);
        var forged = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = Token(clientConfirm), ["_handler"] = "remove-file", ["Removal.FileId"] = file.Id.ToString(),
        };
        await client.Client.PostAsync($"/work/{service}?remove={file.Id}", new FormUrlEncodedContent(forged));
        await using (var db = app.Schema.CreateContext())
        {
            Assert.True(await db.ServiceFiles.AnyAsync(f => f.Id == file.Id));
        }

        var confirm = await accountant.Client.GetHtmlAsync($"/work/{service}?remove={file.Id}");
        Assert.Contains("Remove checklist.png?", confirm);
        Assert.Contains("This removes it from the service for both of you. It can't be undone.", confirm);
        var removed = await accountant.Client.PostAsync($"/work/{service}?remove={file.Id}", new FormUrlEncodedContent(HiddenFields(confirm, "Remove checklist.png?")));
        Assert.Equal($"/work/{service}?done=removed", Target(removed));

        var after = await client.Client.GetHtmlAsync($"/work/{service}");
        Assert.Contains("No documents attached yet.", after);
        Assert.Contains("Removed checklist.png from this service.", after);
        Assert.False(app.Factory.Documents.Objects.ContainsKey(file.ObjectName));
        Assert.Equal(HttpStatusCode.NotFound, (await client.Client.GetAsync(WorkEndpoints.FilePath(service, file.Id))).StatusCode);
    }

    [DatabaseFact]
    public async Task A_storage_failure_says_so_and_lists_nothing()
    {
        var (_, client, service) = await AcceptedAsync("Audit");
        app.Factory.Documents.FailWrites = true;
        try
        {
            var response = await client.Client.PostAsync(WorkEndpoints.FilesPath(service), await UploadFormAsync(client, service, SamplePdf, "ledger.pdf"));
            Assert.Equal($"/work/{service}?file=failed#documents", Target(response));
        }
        finally
        {
            app.Factory.Documents.FailWrites = false;
        }

        Assert.Contains("The document could not be uploaded. Choose the file again and retry.", await client.Client.GetHtmlAsync($"/work/{service}?file=failed"));
        await using var db = app.Schema.CreateContext();
        Assert.False(await db.ServiceFiles.AnyAsync(f => f.EngagementId == service));
        Assert.Contains("Document upload failed in storage (DocumentStorageException)", string.Join("\n", app.Factory.Logs.Messages));
        Assert.DoesNotContain(app.Factory.Logs.Messages, m => m.Contains("ledger", StringComparison.Ordinal) || m.Contains(service.ToString("N"), StringComparison.Ordinal));
    }

    [DatabaseFact]
    public async Task Deleting_an_account_deletes_the_documents_it_uploaded_and_keeps_the_others()
    {
        var (accountant, client, service) = await AcceptedAsync("Tax filing");
        await client.Client.PostAsync(WorkEndpoints.FilesPath(service), await UploadFormAsync(client, service, SamplePdf, "mine.pdf"));
        await accountant.Client.PostAsync(WorkEndpoints.FilesPath(service), await UploadFormAsync(accountant, service, SamplePdf, "theirs.pdf"));
        await using var before = app.Schema.CreateContext();
        var clientFile = await before.ServiceFiles.SingleAsync(f => f.EngagementId == service && f.UploaderAccountId == client.Id);

        var confirm = await client.Client.GetHtmlAsync("/account/delete?step=confirm");
        Assert.Equal("/account/delete?done=1", Target(await client.Client.PostAsync("/account/delete?step=confirm", new FormUrlEncodedContent(HiddenFields(confirm, "Are you sure?")))));

        var page = await accountant.Client.GetHtmlAsync($"/work/{service}");
        Assert.DoesNotContain("<strong>mine.pdf</strong>", page);
        Assert.Contains("<strong>theirs.pdf</strong>", page);
        Assert.Contains("This account was deleted, so the document it added was removed.", page);
        Assert.False(app.Factory.Documents.Objects.ContainsKey(clientFile.ObjectName));
        await using var db = app.Schema.CreateContext();
        Assert.Equal(new[] { "theirs.pdf" }, await db.ServiceFiles.Where(f => f.EngagementId == service).Select(f => f.DisplayName).ToListAsync());
    }

    [DatabaseFact]
    public async Task Without_document_storage_the_page_says_uploads_are_not_set_up()
    {
        await using var factory = new TaxBuddyAppFactory(app.Schema.ConnectionString, withDocumentStorage: false);
        var (_, client, service) = await AcceptedAsync("Payroll");
        var session = factory.CreateHttpsClient();
        session.DefaultRequestHeaders.Add("Cookie", client.Session);

        var page = await session.GetHtmlAsync($"/work/{service}");
        Assert.Contains("Document upload isn't set up on this site yet.", page);
        Assert.DoesNotContain("Upload document", page);
    }

    private sealed record Person(HttpClient Client, Guid Id, string Session);

    private async Task<Person> RegisterAsync(AccountRole role, string name)
    {
        var email = $"{Guid.NewGuid():N}@example.test";
        var client = app.Factory.CreateHttpsClient();
        var session = await TestAccounts.RegisterAsync(client, role, name, email);
        await using var db = app.Schema.CreateContext();
        return new Person(client, (await db.Users.SingleAsync(u => u.Email == email)).Id, session);
    }

    private async Task<AccountantId> ListProfileAsync(Guid owner, string name)
    {
        var profile = AccountantProfile.CreateFor(owner, name, "nina@example.test");
        profile.Edit(name, "Certified Public Accountant", "Taguig City", "Synthetic summary.", "nina@example.test", null, null, true);
        profile.ReplaceServices([("Bookkeeping", ""), ("Tax filing", ""), ("Payroll", ""), ("Audit", ""), ("Business registration", "")]);
        profile.MarkSaved();
        await using var db = app.Schema.CreateContext();
        db.AccountantProfiles.Add(profile);
        await db.SaveChangesAsync();
        return profile.Id;
    }

    private async Task<(Person Accountant, Person Client, Guid Service)> RequestAsync(string service)
    {
        var accountant = await RegisterAsync(AccountRole.Accountant, "Nina Ocampo");
        var profile = await ListProfileAsync(accountant.Id, "Nina Ocampo");
        var client = await RegisterAsync(AccountRole.Client, "Olive Reyes");
        var page = await client.Client.GetHtmlAsync($"/accountants/{profile.Value}/request-service");
        var fields = HiddenFields(page, "will review your request");
        fields["Form.Service"] = service;
        var target = Target(await client.Client.PostAsync($"/accountants/{profile.Value}/request-service", new FormUrlEncodedContent(fields)))!;
        Assert.EndsWith("?done=requested", target);
        return (accountant, client, Guid.Parse(target["/work/".Length..target.IndexOf('?')]));
    }

    private async Task<(Person Accountant, Person Client, Guid Service)> AcceptedAsync(string service)
    {
        var (accountant, client, id) = await RequestAsync(service);
        var page = await accountant.Client.GetHtmlAsync($"/work/{id}");
        Assert.EndsWith("done=accepted", Target(await accountant.Client.PostAsync($"/work/{id}", new FormUrlEncodedContent(HiddenFields(page, "Review this request")))));
        return (accountant, client, id);
    }

    private async Task<Guid> TaskIdAsync(Guid service, int position)
    {
        await using var db = app.Schema.CreateContext();
        return await db.ServiceTasks.Where(t => t.EngagementId == service).OrderBy(t => t.Position).Skip(position).Select(t => t.Id).FirstAsync();
    }

    private async Task<ServiceFile> SingleFileAsync(Guid service)
    {
        await using var db = app.Schema.CreateContext();
        return await db.ServiceFiles.AsNoTracking().SingleAsync(f => f.EngagementId == service);
    }

    /// <summary>The service page's upload form, as a browser posts it: multipart with the page's token and one-time key.</summary>
    private static async Task<MultipartFormDataContent> UploadFormAsync(Person person, Guid service, byte[] content, string name, Guid? task = null) =>
        UploadForm(await person.Client.GetHtmlAsync($"/work/{service}"), content, name, task);

    private static MultipartFormDataContent UploadForm(string page, byte[] content, string name, Guid? task = null)
    {
        var form = new MultipartFormDataContent();
        foreach (var (field, value) in HiddenFields(page, "id=\"documents-heading\""))
        {
            form.Add(new StringContent(value), field);
        }

        form.Add(new StringContent(task?.ToString() ?? ""), "taskId");
        var file = new ByteArrayContent(content);
        file.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream"); // what the browser claims doesn't count
        form.Add(file, "file", name);
        return form;
    }
}
