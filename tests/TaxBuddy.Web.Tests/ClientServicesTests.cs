using System.Net;
using Microsoft.EntityFrameworkCore;
using TaxBuddy.TestSupport;
using TaxBuddy.Web.Domain;
using TaxBuddy.Web.Services;
using TaxBuddy.Web.Tests.Support;
using static TaxBuddy.Web.Tests.Support.FormPosts;

namespace TaxBuddy.Web.Tests;

/// <summary>
/// F03 stage 1 through the whole app (CS01–CS05; PRD R19–R22, BR14–BR16): saving accountants, requesting, accepting and
/// declining, the checklist with optimistic concurrency, shared notes, the two workspace lists, and the access rules.
/// </summary>
public sealed class ClientServicesTests(EmptyAppFixture app) : IClassFixture<EmptyAppFixture>
{
    [DatabaseFact]
    public async Task A_client_adds_an_accountant_once_privately_and_can_remove_them()
    {
        var accountant = await RegisterAsync(AccountRole.Accountant, "Nina Ocampo");
        var profile = await ListProfileAsync(accountant.Id, "Nina Ocampo");
        var client = await RegisterAsync(AccountRole.Client, "Olive Reyes");

        var page = await client.Client.GetHtmlAsync($"/accountants/{profile}");
        Assert.Contains(">Add accountant</button>", page);
        Assert.Contains($"href=\"/accountants/{profile}/request-service\">Request a service</a>", page);
        var add = HiddenFields(page, "work-profile-actions");
        Assert.Equal($"/accountants/{profile}?saved=added", Target(await client.Client.PostAsync(WorkEndpoints.SavedPath, new FormUrlEncodedContent(add))));
        await client.Client.PostAsync(WorkEndpoints.SavedPath, new FormUrlEncodedContent(add)); // Twice: still one.

        Assert.Contains("Added · My accountants", await client.Client.GetHtmlAsync($"/accountants/{profile}"));
        var workspace = await client.Client.GetHtmlAsync("/workspace");
        Assert.Contains("Nina Ocampo", workspace);
        Assert.Contains("No services requested yet.", workspace);
        await using (var db = app.Schema.CreateContext())
        {
            Assert.Equal(1, await db.SavedAccountants.CountAsync(s => s.ClientAccountId == client.Id));
            Assert.False(await db.ServiceEngagements.AnyAsync(e => e.ClientAccountId == client.Id)); // Saving shares nothing.
        }

        Assert.DoesNotContain("Olive Reyes", await accountant.Client.GetHtmlAsync("/workspace"));

        var remove = HiddenFields(workspace, "Nina Ocampo");
        await client.Client.PostAsync(WorkEndpoints.SavedPath, new FormUrlEncodedContent(remove));
        Assert.Contains("No accountants added yet", await client.Client.GetHtmlAsync("/workspace"));
    }

    [DatabaseFact]
    public async Task A_request_is_pending_until_the_accountant_accepts_it_with_a_starting_checklist()
    {
        var (accountant, client, service) = await RequestAsync("Bookkeeping");

        var asClient = await client.Client.GetHtmlAsync($"/work/{service}");
        Assert.Contains("Awaiting acceptance", asClient);
        Assert.Contains("Your accountant has not accepted this request yet.", asClient);
        Assert.DoesNotContain("Accept request", asClient);

        var clients = await accountant.Client.GetHtmlAsync("/workspace");
        Assert.Contains("Olive Reyes", clients);
        Assert.Contains("1 service · 1 awaiting your response", clients);

        var asAccountant = await accountant.Client.GetHtmlAsync($"/work/{service}");
        var accepted = await accountant.Client.PostAsync($"/work/{service}", new FormUrlEncodedContent(HiddenFields(asAccountant, "Review this request")));
        Assert.Equal($"/work/{service}?done=accepted", Target(accepted));

        var active = await client.Client.GetHtmlAsync($"/work/{service}");
        Assert.Contains("Not started", active);
        Assert.Contains("0 of 3 tasks complete", active);
        foreach (var task in WorkRules.StartingTasks)
        {
            Assert.Contains(task, active);
        }

        Assert.Contains("Accepted the service request and created a starting checklist.", active);
        Assert.DoesNotContain("Save checklist changes", active); // The client reads; the accountant edits.
    }

    [DatabaseFact]
    public async Task A_declined_request_stays_distinct_and_the_same_service_can_be_requested_again()
    {
        var (accountant, client, service) = await RequestAsync("Tax filing");
        var page = await accountant.Client.GetHtmlAsync($"/work/{service}");
        Assert.Equal($"/work/{service}?done=declined", Target(await accountant.Client.PostAsync($"/work/{service}",
            new FormUrlEncodedContent(HiddenFields(page, ">Accept request</button>")))));

        Assert.Contains("The accountant declined this request.", await client.Client.GetHtmlAsync($"/work/{service}"));

        var again = await SendRequestAsync(client, accountant.ProfileId!.Value, "Tax filing");
        Assert.StartsWith("/work/", again);
        Assert.EndsWith("?done=requested", again);
        Assert.DoesNotContain(service.ToString(), again);

        var duplicate = await SendRequestAsync(client, accountant.ProfileId!.Value, "Tax filing");
        Assert.Equal(again.Replace("requested", "existing"), duplicate); // An open request isn't duplicated.
    }

    [DatabaseFact]
    public async Task The_accountant_saves_statuses_adds_a_task_and_posts_a_note_that_the_client_sees()
    {
        var (accountant, client, service) = await AcceptedAsync("Payroll");

        var page = await accountant.Client.GetHtmlAsync($"/work/{service}");
        var fields = HiddenFields(page, "Service checklist");
        fields["Statuses.Tasks[0].Status"] = nameof(ServiceTaskStatus.Completed);
        fields["Statuses.Tasks[1].Status"] = nameof(ServiceTaskStatus.WaitingForClient);
        fields["Statuses.Tasks[2].Status"] = nameof(ServiceTaskStatus.NotStarted);
        Assert.Equal($"/work/{service}?done=saved", Target(await accountant.Client.PostAsync($"/work/{service}", new FormUrlEncodedContent(fields))));

        page = await accountant.Client.GetHtmlAsync($"/work/{service}");
        var task = HiddenFields(page, "Save checklist changes");
        task["NewTask.Title"] = "Send payslips";
        Assert.Equal($"/work/{service}?done=task", Target(await accountant.Client.PostAsync($"/work/{service}", new FormUrlEncodedContent(task))));
        page = await accountant.Client.GetHtmlAsync($"/work/{service}");
        var note = HiddenFields(page, "Progress updates");
        note["Note.Text"] = "Please send the March timesheets.";
        Assert.Equal($"/work/{service}?done=note", Target(await accountant.Client.PostAsync($"/work/{service}", new FormUrlEncodedContent(note))));

        var seen = await client.Client.GetHtmlAsync($"/work/{service}");
        Assert.Contains("Waiting for client", seen);
        Assert.Contains("1 of 4 tasks complete · 25%", seen);
        Assert.Contains("Send payslips", seen);
        Assert.Contains("Please send the March timesheets.", seen);
        Assert.Contains("Agree the service requirements: Completed; Prepare the work: Waiting for client", seen);
        var mine = await client.Client.GetHtmlAsync("/workspace");
        Assert.Contains("Payroll", mine);
        Assert.Contains("1 of 4 tasks complete", mine);
    }

    [DatabaseFact]
    public async Task A_stale_checklist_save_is_refused_and_keeps_the_choices()
    {
        var (accountant, _, service) = await AcceptedAsync("Audit");
        var first = HiddenFields(await accountant.Client.GetHtmlAsync($"/work/{service}"), "Service checklist");
        var second = new Dictionary<string, string>(first);

        first["Statuses.Tasks[0].Status"] = nameof(ServiceTaskStatus.InProgress);
        await accountant.Client.PostAsync($"/work/{service}", new FormUrlEncodedContent(first));
        second["Statuses.Tasks[0].Status"] = nameof(ServiceTaskStatus.Completed);
        var stale = await accountant.Client.PostAsync($"/work/{service}", new FormUrlEncodedContent(second));
        var html = await stale.ReadHtmlAsync();

        Assert.Equal(HttpStatusCode.OK, stale.StatusCode);
        Assert.Contains("Someone saved this checklist since you opened it.", html);
        Assert.Contains("Saved now: In progress", html);
        Assert.Matches("""<option value="Completed" selected""", html);
        await using var db = app.Schema.CreateContext();
        Assert.Equal(ServiceTaskStatus.InProgress, (await db.ServiceTasks.Where(t => t.EngagementId == service).OrderBy(t => t.Position).FirstAsync()).Status);
    }

    [DatabaseFact]
    public async Task Sending_the_same_task_or_note_form_twice_stores_it_once()
    {
        // PRD D31 (2026-11-25): a double click sends a static form twice, and the second post changes nothing (BR18).
        var (accountant, _, service) = await AcceptedAsync("Bookkeeping");
        var (task, note) = TaskAndNoteForms(await accountant.Client.GetHtmlAsync($"/work/{service}"));

        for (var i = 0; i < 2; i++)
        {
            Assert.Equal($"/work/{service}?done=task", Target(await accountant.Client.PostAsync($"/work/{service}", new FormUrlEncodedContent(task))));
            Assert.Equal($"/work/{service}?done=note", Target(await accountant.Client.PostAsync($"/work/{service}", new FormUrlEncodedContent(note))));
        }

        await AssertStoredOnceAsync(service);
    }

    [DatabaseFact]
    public async Task The_same_task_or_note_form_sent_twice_at_once_is_stored_once()
    {
        // The two posts race: the one that loses finds the other's row and answers "done" too (PRD D31).
        var (accountant, _, service) = await AcceptedAsync("Payroll");
        var (task, note) = TaskAndNoteForms(await accountant.Client.GetHtmlAsync($"/work/{service}"));

        var responses = await Task.WhenAll(new[] { task, task, note, note }
            .Select(form => accountant.Client.PostAsync($"/work/{service}", new FormUrlEncodedContent(form))));

        Assert.Equal(["task", "task", "note", "note"], responses.Select(r => Target(r)?.Replace($"/work/{service}?done=", "")));
        await AssertStoredOnceAsync(service);
    }

    [DatabaseFact]
    public async Task A_different_note_sent_from_an_older_copy_of_the_page_is_shared_too()
    {
        // After the browser's Back, the page can return with a key already used. A different text is a new note, not a repeat.
        var (accountant, _, service) = await AcceptedAsync("Audit");
        var note = HiddenFields(await accountant.Client.GetHtmlAsync($"/work/{service}"), "Progress updates");
        foreach (var text in new[] { "First update.", "Second update." })
        {
            note["Note.Text"] = text;
            Assert.Equal($"/work/{service}?done=note", Target(await accountant.Client.PostAsync($"/work/{service}", new FormUrlEncodedContent(note))));
        }

        await using var db = app.Schema.CreateContext();
        Assert.True(await db.ServiceUpdates.AnyAsync(u => u.EngagementId == service && u.Text == "First update."));
        Assert.True(await db.ServiceUpdates.AnyAsync(u => u.EngagementId == service && u.Text == "Second update."));
    }

    [DatabaseFact]
    public async Task A_note_form_without_its_key_is_refused_with_the_text_kept()
    {
        // A page drawn before the key existed can't be checked for repeats, so it is drawn again with a key (PRD D31).
        var (accountant, _, service) = await AcceptedAsync("Tax filing");
        var note = HiddenFields(await accountant.Client.GetHtmlAsync($"/work/{service}"), "Progress updates");
        Assert.True(note.Remove("Note.Key"));
        note["Note.Text"] = "Sent without a key.";

        var response = await accountant.Client.PostAsync($"/work/{service}", new FormUrlEncodedContent(note));
        var html = await response.ReadHtmlAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("We couldn't save that. Your changes are kept; please try again.", html);
        Assert.Contains(">Sent without a key.</textarea>", html);
        Assert.Matches("""<input type="hidden" name="Note.Key" value="[0-9a-f]{8}-[0-9a-f-]{27}" """, html);
        await using var db = app.Schema.CreateContext();
        Assert.False(await db.ServiceUpdates.AnyAsync(u => u.EngagementId == service && u.Text == "Sent without a key."));
    }

    [DatabaseFact]
    public async Task Accepting_or_declining_a_request_twice_changes_nothing_more_and_says_done()
    {
        // PRD D31: the second post of a double click finds the request already answered. It got an empty 400, because the
        // page no longer drew the form it was sent from.
        var (accountant, _, accepted) = await RequestAsync("Bookkeeping");
        var requestPage = await accountant.Client.GetHtmlAsync($"/work/{accepted}");
        var accept = HiddenFields(requestPage, "Review this request");
        var lateDecline = HiddenFields(requestPage, ">Accept request</button>");
        var (other, _, declined) = await RequestAsync("Tax filing");
        var decline = HiddenFields(await other.Client.GetHtmlAsync($"/work/{declined}"), ">Accept request</button>");

        for (var i = 0; i < 2; i++)
        {
            Assert.Equal($"/work/{accepted}?done=accepted", Target(await accountant.Client.PostAsync($"/work/{accepted}", new FormUrlEncodedContent(accept))));
            Assert.Equal($"/work/{declined}?done=declined", Target(await other.Client.PostAsync($"/work/{declined}", new FormUrlEncodedContent(decline))));
        }

        // Decline from the same old page after accepting: it no longer applies, so the service is shown as it is.
        Assert.Equal($"/work/{accepted}", Target(await accountant.Client.PostAsync($"/work/{accepted}", new FormUrlEncodedContent(lateDecline))));
        Assert.Contains("Not started", await accountant.Client.GetHtmlAsync($"/work/{accepted}"));

        await AssertAcceptedOnceAsync(accepted);
        await using var db = app.Schema.CreateContext();
        Assert.Equal(1, await db.ServiceUpdates.CountAsync(u => u.EngagementId == declined && u.Text == "Declined the service request."));
    }

    [DatabaseFact]
    public async Task Accepting_a_request_twice_at_once_accepts_it_once()
    {
        // The post that loses the race tries again against the saved service, and finds it already accepted.
        var (accountant, _, service) = await RequestAsync("Payroll");
        var accept = HiddenFields(await accountant.Client.GetHtmlAsync($"/work/{service}"), "Review this request");

        var responses = await Task.WhenAll(Enumerable.Range(0, 2)
            .Select(_ => accountant.Client.PostAsync($"/work/{service}", new FormUrlEncodedContent(accept))));

        Assert.All(responses, r => Assert.Equal($"/work/{service}?done=accepted", Target(r)));
        await AssertAcceptedOnceAsync(service);
    }

    [DatabaseFact]
    public async Task Saving_the_same_checklist_twice_says_saved_and_records_it_once()
    {
        // PRD D31: the second post carries the version from before the first, but asks for nothing that isn't saved now.
        // It said "Someone saved this checklist since you opened it". A real conflict still does (the stale test above).
        var (accountant, _, service) = await AcceptedAsync("Audit");
        var first = HiddenFields(await accountant.Client.GetHtmlAsync($"/work/{service}"), "Service checklist");
        first["Statuses.Tasks[0].Status"] = nameof(ServiceTaskStatus.Completed);
        for (var i = 0; i < 2; i++)
        {
            Assert.Equal($"/work/{service}?done=saved", Target(await accountant.Client.PostAsync($"/work/{service}", new FormUrlEncodedContent(first))));
        }

        var second = HiddenFields(await accountant.Client.GetHtmlAsync($"/work/{service}"), "Service checklist");
        second["Statuses.Tasks[1].Status"] = nameof(ServiceTaskStatus.InProgress);
        var responses = await Task.WhenAll(Enumerable.Range(0, 2)
            .Select(_ => accountant.Client.PostAsync($"/work/{service}", new FormUrlEncodedContent(second))));
        Assert.All(responses, r => Assert.Equal($"/work/{service}?done=saved", Target(r)));

        await using var db = app.Schema.CreateContext();
        Assert.Equal(1, await db.ServiceUpdates.CountAsync(u => u.EngagementId == service
            && u.Text == $"Agree the service requirements: {WorkRules.Label(ServiceTaskStatus.Completed)}"));
        Assert.Equal(1, await db.ServiceUpdates.CountAsync(u => u.EngagementId == service
            && u.Text == $"Prepare the work: {WorkRules.Label(ServiceTaskStatus.InProgress)}"));
    }

    [DatabaseFact]
    public async Task Nobody_else_sees_or_changes_a_service()
    {
        var (accountant, client, service) = await AcceptedAsync("Business registration");
        var stranger = await RegisterAsync(AccountRole.Client, "Rina Cruz");
        var otherAccountant = await RegisterAsync(AccountRole.Accountant, "Paolo Lim");

        foreach (var other in new[] { stranger, otherAccountant })
        {
            var response = await other.Client.GetAsync($"/work/{service}");
            var html = await response.ReadHtmlAsync();
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("This workspace is not available", html);
            Assert.DoesNotContain("Olive", html);
            Assert.DoesNotContain("Business registration", html);
        }

        Assert.Equal(HttpStatusCode.NotFound, (await otherAccountant.Client.GetAsync($"/clients/{client.Id}")).StatusCode);
        Assert.Contains("Sign in to see your workspace", await app.Factory.CreateHttpsClient().GetHtmlAsync($"/work/{service}"));

        // The client can't post the accountant's forms: the page draws none, and a forged post changes nothing.
        var clientPage = await client.Client.GetHtmlAsync($"/work/{service}");
        var forged = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = Token(clientPage), ["_handler"] = "note", ["Note.Text"] = "Forged",
        };
        await client.Client.PostAsync($"/work/{service}", new FormUrlEncodedContent(forged));
        await using var db = app.Schema.CreateContext();
        Assert.False(await db.ServiceUpdates.AnyAsync(u => u.EngagementId == service && u.Text == "Forged"));
    }

    [DatabaseFact]
    public async Task A_failed_add_or_remove_is_shown_as_an_error()
    {
        // 2026-11-24: the prototype's red .work-error, on My accountants and on the profile; it was a green success line.
        var client = await RegisterAsync(AccountRole.Client, "Olive Reyes");
        var profile = (await RegisterListedAsync("Rico Santos")).ProfileId!.Value;
        const string failedLine = """<p class="work-error" role="alert">We couldn't change your list. Please try again.</p>""";

        foreach (var page in new[] { "/workspace?saved=failed", $"/accountants/{profile.Value}?saved=failed" })
        {
            var html = await client.Client.GetHtmlAsync(page);
            Assert.Contains(failedLine, html);
            Assert.DoesNotContain("save-status ok", html);
        }
    }

    [DatabaseFact]
    public async Task Workspace_pages_use_the_prototype_frame()
    {
        // 2026-11-24: the full page width with the breadcrumb and h1 (prototype desktopMain), not the 680 px column.
        var (accountant, client, service) = await AcceptedAsync("Bookkeeping");

        var list = await client.Client.GetHtmlAsync("/workspace");
        Assert.Contains("""<nav class="crumbs" aria-label="Breadcrumb"><a href="/workspace">My accountants</a></nav>""", list);
        Assert.Contains("""<h1 id="page-title" tabindex="-1" class="work-page-title">My accountants</h1>""", list);
        Assert.DoesNotContain("single-inner", list);

        var work = await accountant.Client.GetHtmlAsync($"/work/{service}");
        Assert.Matches("""<a href="/workspace">Clients</a>\s*<span aria-hidden="true">/</span>\s*<span>Service progress</span>""", work);
    }

    [DatabaseFact]
    public async Task The_site_bar_marks_the_workspace_on_service_client_and_request_pages()
    {
        // 2026-11-24, prototype desktopShell: these pages belong to the workspace, not the directory; the review page to none.
        var (accountant, client, service) = await AcceptedAsync("Bookkeeping");
        var profile = (await RegisterListedAsync("Rico Santos")).ProfileId!.Value;
        const string current = """<a href="/workspace" class="active" aria-current="page"[^>]*>""";

        Assert.Matches(current, await accountant.Client.GetHtmlAsync($"/work/{service}"));
        Assert.Matches(current, await accountant.Client.GetHtmlAsync($"/clients/{client.Id}"));
        var request = await client.Client.GetHtmlAsync($"/accountants/{profile.Value}/request-service");
        Assert.Matches(current, request);
        Assert.DoesNotMatch("""class="active" aria-current="page"[^>]*>Find an accountant""", request);
        var review = await (await client.Client.GetAsync($"/accountants/{profile.Value}/review")).ReadHtmlAsync();
        Assert.Contains("Write a review", review);
        Assert.DoesNotContain("""class="active" aria-current="page""", review);
    }

    [DatabaseFact]
    public async Task Unavailable_requests_and_malformed_addresses_keep_the_workspace_frame()
    {
        var accountant = await RegisterAsync(AccountRole.Accountant, "Nina Ocampo");
        var profile = (await RegisterListedAsync("Rico Santos")).ProfileId!.Value;

        // An accountant can't request a service: the workspace's own 404 answer (DESIGN), not a page of its own.
        var asAccountant = await accountant.Client.GetAsync($"/accountants/{profile.Value}/request-service");
        Assert.Equal(HttpStatusCode.NotFound, asAccountant.StatusCode);
        Assert.Contains("This workspace is not available", await asAccountant.ReadHtmlAsync());

        // A malformed address keeps the frame; a visitor is asked to sign in first (SDD private workspaces).
        var malformed = await accountant.Client.GetAsync("/clients/not-an-id");
        var malformedHtml = await malformed.ReadHtmlAsync();
        Assert.Equal(HttpStatusCode.NotFound, malformed.StatusCode);
        Assert.Contains("""<h1 id="page-title" tabindex="-1" class="work-page-title">Client services</h1>""", malformedHtml);
        Assert.Contains("This workspace is not available", malformedHtml);
        var visitor = await app.Factory.CreateHttpsClient().GetAsync("/work/not-an-id");
        Assert.Equal(HttpStatusCode.NotFound, visitor.StatusCode);
        Assert.Contains("Sign in to see your workspace", await visitor.ReadHtmlAsync());
    }

    [DatabaseFact]
    public async Task An_accountant_adds_a_service_for_a_client_they_already_serve()
    {
        var (accountant, client, _) = await AcceptedAsync("Bookkeeping");

        var page = await accountant.Client.GetHtmlAsync($"/clients/{client.Id}");
        Assert.Contains("Add a service for this client", page);
        Assert.DoesNotContain("<option value=\"Bookkeeping\"", page); // Already open.
        var fields = HiddenFields(page, "Add a service for this client");
        fields["Form.Service"] = "Payroll";
        var added = await accountant.Client.PostAsync($"/clients/{client.Id}", new FormUrlEncodedContent(fields));

        Assert.EndsWith("?done=added", Target(added));
        var service = await client.Client.GetHtmlAsync(Target(added)!);
        Assert.Contains("Payroll", service);
        Assert.Contains("0 of 3 tasks complete", service);
        Assert.Contains("Added Payroll to this client relationship.", service);
    }

    [DatabaseFact]
    public async Task The_navigation_names_the_workspace_by_role()
    {
        var accountant = await RegisterAsync(AccountRole.Accountant, "Nina Ocampo");
        var client = await RegisterAsync(AccountRole.Client, "Olive Reyes");

        Assert.Matches("""<a href="/workspace"[^>]*>Clients</a>""", await accountant.Client.GetHtmlAsync("/"));
        Assert.Matches("""<a href="/workspace"[^>]*>My accountants</a>""", await client.Client.GetHtmlAsync("/"));
        Assert.DoesNotContain("href=\"/workspace\"", await app.Factory.CreateHttpsClient().GetHtmlAsync("/"));
    }

    [DatabaseFact]
    public async Task Account_opens_the_workspace_by_role_as_its_first_row()
    {
        // Prototype meHTML: the workspace row comes first, then the accountant's profile row; "My reviews" stays out (F09).
        var accountant = await RegisterAsync(AccountRole.Accountant, "Nina Ocampo");
        var client = await RegisterAsync(AccountRole.Client, "Olive Reyes");
        const string detail = "<small>Private checklists, shared updates and service documents</small>";

        var asAccountant = await accountant.Client.GetHtmlAsync("/account");
        Assert.Contains($"<span class=\"mi-main\">Clients and service work{detail}</span>", asAccountant);
        Assert.True(asAccountant.IndexOf("Clients and service work", StringComparison.Ordinal) < asAccountant.IndexOf("My profile and portfolio", StringComparison.Ordinal));
        Assert.DoesNotContain("My reviews", asAccountant);

        var asClient = await client.Client.GetHtmlAsync("/account");
        Assert.Contains($"<span class=\"mi-main\">My accountants and service progress{detail}</span>", asClient);
        Assert.DoesNotContain("Clients and service work", asClient);

        foreach (var page in new[] { asAccountant, asClient })
        {
            Assert.Matches("""<a class="mi" href="/workspace">\s*<svg""", page);
        }

        var visitor = await app.Factory.CreateHttpsClient().GetHtmlAsync("/account");
        Assert.Contains("You're browsing as a visitor", visitor);
        Assert.DoesNotContain("<a class=\"mi\" href=\"/workspace\">", visitor);
    }

    private sealed record Person(HttpClient Client, Guid Id, AccountantId? ProfileId = null);

    private async Task<Person> RegisterAsync(AccountRole role, string name)
    {
        var email = $"{Guid.NewGuid():N}@example.test";
        var client = app.Factory.CreateHttpsClient();
        await TestAccounts.RegisterAsync(client, role, name, email);
        await using var db = app.Schema.CreateContext();
        return new Person(client, (await db.Users.SingleAsync(u => u.Email == email)).Id);
    }

    private async Task<Person> RegisterListedAsync(string name)
    {
        var accountant = await RegisterAsync(AccountRole.Accountant, name);
        return accountant with { ProfileId = await ListProfileAsync(accountant.Id, name) };
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

    private static async Task<string> SendRequestAsync(Person client, AccountantId profile, string service)
    {
        var page = await client.Client.GetHtmlAsync($"/accountants/{profile.Value}/request-service");
        var fields = HiddenFields(page, "will review your request");
        fields["Form.Service"] = service;
        return Target(await client.Client.PostAsync($"/accountants/{profile.Value}/request-service", new FormUrlEncodedContent(fields)))!;
    }

    private async Task<(Person Accountant, Person Client, Guid Service)> RequestAsync(string service)
    {
        var accountant = await RegisterAsync(AccountRole.Accountant, "Nina Ocampo");
        var profile = await ListProfileAsync(accountant.Id, "Nina Ocampo");
        accountant = accountant with { ProfileId = profile };
        var client = await RegisterAsync(AccountRole.Client, "Olive Reyes");
        var target = await SendRequestAsync(client, profile, service);
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

    /// <summary>The accountant's "Add task" and shared-note forms, filled in, each with the key the page drew.</summary>
    private static (Dictionary<string, string> Task, Dictionary<string, string> Note) TaskAndNoteForms(string page)
    {
        var task = HiddenFields(page, "Save checklist changes");
        task["NewTask.Title"] = "Send payslips";
        var note = HiddenFields(page, "Progress updates");
        note["Note.Text"] = "Please send the March timesheets.";
        Assert.NotEqual(task["NewTask.Key"], note["Note.Key"]);
        return (task, note);
    }

    private async Task AssertAcceptedOnceAsync(Guid service)
    {
        await using var db = app.Schema.CreateContext();
        Assert.Equal(WorkRules.StartingTasks.Length, await db.ServiceTasks.CountAsync(t => t.EngagementId == service));
        Assert.Equal(1, await db.ServiceUpdates.CountAsync(u => u.EngagementId == service
            && u.Text == "Accepted the service request and created a starting checklist."));
    }

    private async Task AssertStoredOnceAsync(Guid service)
    {
        await using var db = app.Schema.CreateContext();
        Assert.Equal(1, await db.ServiceTasks.CountAsync(t => t.EngagementId == service && t.Title == "Send payslips"));
        Assert.Equal(1, await db.ServiceUpdates.CountAsync(u => u.EngagementId == service && u.Text == "Added checklist task: Send payslips"));
        Assert.Equal(1, await db.ServiceUpdates.CountAsync(u => u.EngagementId == service && u.Text == "Please send the March timesheets."));
    }
}
