using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaxBuddy.TestSupport;
using TaxBuddy.Web.Application.Chat;
using TaxBuddy.Web.Domain;
using TaxBuddy.Web.Tests.Support;
using static TaxBuddy.Web.Tests.Support.FormPosts;

namespace TaxBuddy.Web.Tests;

/// <summary>
/// F10 report and block through the whole app (RB01–RB09): the static report page, the report links on profiles, reviews
/// and chats, and the blocked states the chat panel and the review page draw (prerendered). Blocking itself happens in the
/// live panel; here it is set through the chat service, and the browser check presses the buttons.
/// </summary>
public sealed class ReportPageTests(EmptyAppFixture app) : IClassFixture<EmptyAppFixture>
{
    [DatabaseFact]
    public async Task A_visitor_sees_report_this_profile_and_is_asked_to_sign_in()
    {
        var (_, accountantId) = await SignedInAsync(AccountRole.Accountant, "Nina Ocampo");
        var profile = await ListProfileAsync(accountantId, "Nina Ocampo");
        var visitor = app.Factory.CreateHttpsClient();

        var profileHtml = await visitor.GetHtmlAsync($"/accountants/{profile}");
        var report = await visitor.GetAsync($"/report/profile/{profile}");

        Assert.Contains($"href=\"/report/profile/{profile}\">Report this profile</a>", profileHtml);
        Assert.Equal(HttpStatusCode.OK, report.StatusCode);
        var html = await report.ReadHtmlAsync();
        Assert.Contains("Sign in to send a report", html);
        Assert.Contains($"href=\"/signin?returnUrl=%2Freport%2Fprofile%2F{profile}\"", html);
        Assert.DoesNotContain("name=\"Form.Reason\"", html);
        Assert.Contains("Sign in to send a report", await visitor.GetHtmlAsync($"/report/conversation/{Guid.NewGuid()}")); // Nothing revealed.
    }

    [DatabaseFact]
    public async Task A_client_reports_a_profile_once_with_a_reason_and_is_thanked()
    {
        var (_, accountantId) = await SignedInAsync(AccountRole.Accountant, "Nina Ocampo");
        var profile = await ListProfileAsync(accountantId, "Nina Ocampo");
        var (client, clientId) = await SignedInAsync(AccountRole.Client, "Olive Reyes");

        var form = await client.GetHtmlAsync($"/report/profile/{profile}");
        Assert.Contains("Report this profile</h1>", form);
        Assert.Contains("Nina Ocampo's profile", form);
        Assert.Contains("Reports go to the Tax Buddy team, who review them by hand. The other person isn't told who reported them.", form);
        Assert.Equal(5, form.Split("name=\"Form.Reason\"").Length - 1);

        var missing = await PostReportAsync(client, $"/report/profile/{profile}", form, ("Form.Details", "Fake reviews?"));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, missing.StatusCode);
        var missingHtml = await missing.ReadHtmlAsync();
        Assert.Contains("Choose a reason.", missingHtml);
        Assert.Contains("Fake reviews?</textarea>", missingHtml);

        var sent = await PostReportAsync(client, $"/report/profile/{profile}", form, ("Form.Reason", "FakeOrMisleading"), ("Form.Details", "Fake reviews?"));
        Assert.Equal($"/accountants/{profile}?reported=profile", Target(sent));
        Assert.Contains("Thanks. Your report was sent to the Tax Buddy team.", await client.GetHtmlAsync($"/accountants/{profile}?reported=profile"));
        await using (var db = app.Schema.CreateContext())
        {
            var stored = await db.Reports.SingleAsync(r => r.ReporterAccountId == clientId);
            Assert.Equal((ReportKind.Profile, ReportReason.FakeOrMisleading, "Fake reviews?"), (stored.Kind, stored.Reason, stored.Details));
        }

        Assert.Contains("You've already reported this", await client.GetHtmlAsync($"/report/profile/{profile}"));
    }

    [DatabaseFact]
    public async Task The_owner_has_no_report_link_and_cannot_report_their_own_profile()
    {
        var (accountant, accountantId) = await SignedInAsync(AccountRole.Accountant, "Nina Ocampo");
        var profile = await ListProfileAsync(accountantId, "Nina Ocampo");

        Assert.DoesNotContain("Report this profile", await accountant.GetHtmlAsync($"/accountants/{profile}"));
        var own = await accountant.GetAsync($"/report/profile/{profile}");
        Assert.Equal(HttpStatusCode.Forbidden, own.StatusCode);
        Assert.Contains("You can't report your own profile", await own.ReadHtmlAsync());
    }

    [DatabaseFact]
    public async Task Unknown_items_and_other_peoples_conversations_are_not_available()
    {
        var (_, accountantId) = await SignedInAsync(AccountRole.Accountant, "Nina Ocampo");
        var profile = await ListProfileAsync(accountantId, "Nina Ocampo");
        var (_, clientId) = await SignedInAsync(AccountRole.Client, "Olive Reyes");
        var conversation = await SendAsync(new ChatViewer(clientId, AccountRole.Client), profile, null, "Something private");
        var (outsider, _) = await SignedInAsync(AccountRole.Client, "Paolo Tan");

        foreach (var address in new[] { $"/report/conversation/{conversation:D}", $"/report/review/{Guid.NewGuid()}", "/report/user/123", $"/report/profile/{AccountantId.New()}" })
        {
            var response = await outsider.GetAsync(address);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var html = await response.ReadHtmlAsync();
            Assert.Contains("This isn't available", html);
            Assert.DoesNotContain("Olive", html);
        }
    }

    [DatabaseFact]
    public async Task Report_this_review_shows_for_signed_in_users_other_than_its_author()
    {
        var (accountant, accountantId) = await SignedInAsync(AccountRole.Accountant, "Nina Ocampo");
        var profile = await ListProfileAsync(accountantId, "Nina Ocampo");
        var (client, clientId) = await SignedInAsync(AccountRole.Client, "Olive Reyes");
        var conversation = await SendAsync(new ChatViewer(clientId, AccountRole.Client), profile, null, "Hi! Can you help?");
        await SendAsync(new ChatViewer(accountantId, AccountRole.Accountant), null, conversation, "Yes.");
        await PostReviewAsync(client, profile, ("Form.Stars", "4"));
        Guid reviewId;
        await using (var db = app.Schema.CreateContext())
        {
            reviewId = (await db.Reviews.SingleAsync(r => r.AccountantId == profile)).Id;
        }

        var link = $"href=\"/report/review/{reviewId:D}\">Report this review</a>";
        Assert.Contains(link, await accountant.GetHtmlAsync($"/accountants/{profile}"));
        Assert.DoesNotContain(link, await client.GetHtmlAsync($"/accountants/{profile}"));
        Assert.DoesNotContain(link, await app.Factory.CreateHttpsClient().GetHtmlAsync($"/accountants/{profile}"));
        var form = await accountant.GetHtmlAsync($"/report/review/{reviewId:D}");
        Assert.Contains("Olive R.'s review of Nina Ocampo", form);

        var sent = await PostReportAsync(accountant, $"/report/review/{reviewId:D}", form, ("Form.Reason", "AbusiveOrHarassing"));
        Assert.Equal($"/accountants/{profile}?reported=review", Target(sent)); // Then #review-…, which the browser keeps.
    }

    [DatabaseFact]
    public async Task A_blocked_conversation_shows_each_side_its_note_instead_of_the_composer()
    {
        var (accountant, accountantId) = await SignedInAsync(AccountRole.Accountant, "Nina Ocampo");
        var profile = await ListProfileAsync(accountantId, "Nina Ocampo");
        var (client, clientId) = await SignedInAsync(AccountRole.Client, "Olive Reyes");
        var conversation = await SendAsync(new ChatViewer(clientId, AccountRole.Client), profile, null, "Hi! Can you help?");
        await SendAsync(new ChatViewer(accountantId, AccountRole.Accountant), null, conversation, "Yes.");

        var open = await client.GetHtmlAsync($"/chats/{conversation:D}");
        Assert.Contains($"href=\"/report/conversation/{conversation:D}\">Report</a>", open);
        Assert.Contains(">Block</button>", open);
        Assert.Contains("data-chat-composer", open);

        await BlockAsync(accountantId, conversation);

        var blocked = await client.GetHtmlAsync($"/chats/{conversation:D}");
        Assert.Contains("You can't send messages in this conversation.", blocked);
        Assert.DoesNotContain("data-chat-composer", blocked);
        Assert.DoesNotContain("You blocked", blocked); // Not told why.
        var blocker = await accountant.GetHtmlAsync($"/chats/{conversation:D}");
        Assert.Contains("You blocked Olive. They can't message you.", blocker);
        Assert.Contains(">Unblock</button>", blocker);
        Assert.DoesNotContain(">Block</button>", blocker);

        Assert.Contains("Thanks. Your report was sent to the Tax Buddy team.", await client.GetHtmlAsync($"/chats/{conversation:D}?reported=1"));
    }

    [DatabaseFact]
    public async Task A_client_the_accountant_blocked_cannot_review_but_can_delete_their_review()
    {
        var (_, accountantId) = await SignedInAsync(AccountRole.Accountant, "Nina Ocampo");
        var profile = await ListProfileAsync(accountantId, "Nina Ocampo");
        var (client, clientId) = await SignedInAsync(AccountRole.Client, "Olive Reyes");
        var conversation = await SendAsync(new ChatViewer(clientId, AccountRole.Client), profile, null, "Hi! Can you help?");
        await SendAsync(new ChatViewer(accountantId, AccountRole.Accountant), null, conversation, "Yes.");
        await PostReviewAsync(client, profile, ("Form.Stars", "2"));
        await BlockAsync(accountantId, conversation);

        var profileHtml = await client.GetHtmlAsync($"/accountants/{profile}");
        Assert.Contains("You can't review Nina.", profileHtml);
        Assert.DoesNotContain("Edit your review", profileHtml);
        var page = await client.GetAsync($"/accountants/{profile}/review");
        Assert.Equal(HttpStatusCode.Forbidden, page.StatusCode);
        var html = await page.ReadHtmlAsync();
        Assert.Contains("You can't review Nina", html);
        Assert.DoesNotContain("name=\"Form.Stars\"", html);

        var values = HiddenFields(html, "You can't review Nina");
        values["command"] = "delete";
        var deleted = await client.PostAsync($"/accountants/{profile}/review", new FormUrlEncodedContent(values));
        Assert.StartsWith($"/accountants/{profile}?saved=review-deleted", Target(deleted));
        await using var db = app.Schema.CreateContext();
        Assert.False(await db.Reviews.AnyAsync(r => r.AccountantId == profile));
    }

    private static Task<HttpResponseMessage> PostReportAsync(HttpClient client, string address, string formHtml, params (string, string)[] fields)
    {
        var values = HiddenFields(formHtml, "Reports go to the Tax Buddy team");
        foreach (var (name, value) in fields)
        {
            values[name] = value;
        }

        return client.PostAsync(address, new FormUrlEncodedContent(values));
    }

    private static async Task PostReviewAsync(HttpClient client, AccountantId profile, params (string, string)[] fields)
    {
        var values = HiddenFields(await client.GetHtmlAsync($"/accountants/{profile}/review"), "Write a review</h1>");
        values["command"] = "save";
        foreach (var (name, value) in fields)
        {
            values[name] = value;
        }

        Assert.StartsWith($"/accountants/{profile}?saved=review-posted",
            Target(await client.PostAsync($"/accountants/{profile}/review", new FormUrlEncodedContent(values))));
    }

    private async Task BlockAsync(Guid accountId, Guid conversation)
    {
        using var scope = app.Factory.Services.CreateScope();
        Assert.True(await scope.ServiceProvider.GetRequiredService<ChatService>().SetBlockAsync(accountId, conversation, blocked: true));
    }

    /// <summary>A message through the app's own chat service (the live panel's send needs a browser).</summary>
    private async Task<Guid> SendAsync(ChatViewer sender, AccountantId? newChatWith, Guid? conversation, string text)
    {
        using var scope = app.Factory.Services.CreateScope();
        var sent = await scope.ServiceProvider.GetRequiredService<ChatService>()
            .SendAsync(sender, conversation, newChatWith, Guid.NewGuid(), text);
        Assert.Equal(SendOutcome.Sent, sent.Outcome);
        return sent.ConversationId!.Value;
    }

    private async Task<(HttpClient Client, Guid AccountId)> SignedInAsync(AccountRole role, string name)
    {
        var email = $"{Guid.NewGuid():N}@example.test";
        var client = app.Factory.CreateHttpsClient();
        await TestAccounts.RegisterAsync(client, role, name, email);
        await using var db = app.Schema.CreateContext();
        return (client, (await db.Users.SingleAsync(u => u.Email == email)).Id);
    }

    private async Task<AccountantId> ListProfileAsync(Guid owner, string name)
    {
        var profile = AccountantProfile.CreateFor(owner, name, "nina@example.test");
        profile.Edit(name, "Certified Public Accountant", "Taguig City", "Synthetic summary.", "nina@example.test", null, null, true);
        profile.ReplaceServices([("Bookkeeping", "")]);
        profile.MarkSaved();
        await using var db = app.Schema.CreateContext();
        db.AccountantProfiles.Add(profile);
        await db.SaveChangesAsync();
        return profile.Id;
    }
}
