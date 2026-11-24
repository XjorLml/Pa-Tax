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
/// F09 reviews through the whole app (RV01–RV08): the static review form, the profile's Reviews section, the owner's reply
/// form and the directory's rating line and order. Posts are made as a browser would, including forged ones.
/// </summary>
public sealed class ReviewPageTests(EmptyAppFixture app) : IClassFixture<EmptyAppFixture>
{
    [DatabaseFact]
    public async Task A_visitor_sees_no_reviews_yet_and_is_asked_to_sign_in_to_review()
    {
        var (_, accountantId) = await SignedInAsync(AccountRole.Accountant, "Nina Ocampo");
        var profile = await ListProfileAsync(accountantId, "Nina Ocampo");
        var visitor = app.Factory.CreateHttpsClient();

        var profileHtml = await visitor.GetHtmlAsync($"/accountants/{profile}");
        var review = await visitor.GetAsync($"/accountants/{profile}/review");

        Assert.Contains("<span class=\"muted\">No reviews yet</span>", profileHtml); // The head's rating line.
        Assert.Contains("No reviews yet.", profileHtml);
        Assert.Contains("Reviews come from clients Nina has chatted with.", profileHtml);
        Assert.DoesNotContain("Write a review", profileHtml);
        Assert.DoesNotContain("out of 5 stars", profileHtml); // Never a zero score (BR10).
        Assert.Equal(HttpStatusCode.OK, review.StatusCode);
        var html = await review.ReadHtmlAsync();
        Assert.Contains("Sign in to write a review", html);
        Assert.Contains($"href=\"/signin?returnUrl=%2Faccountants%2F{profile}%2Freview\"", html);
        Assert.DoesNotContain("name=\"Form.Stars\"", html);
    }

    [DatabaseFact]
    public async Task A_client_the_accountant_hasnt_replied_to_sees_why_and_a_forged_post_saves_nothing()
    {
        var (_, accountantId) = await SignedInAsync(AccountRole.Accountant, "Nina Ocampo");
        var profile = await ListProfileAsync(accountantId, "Nina Ocampo");
        var (client, clientId) = await SignedInAsync(AccountRole.Client, "Olive Reyes");
        await SendAsync(new ChatViewer(clientId, AccountRole.Client), profile, null, "Hi! Can you help?");

        var review = await client.GetAsync($"/accountants/{profile}/review");
        Assert.Equal(HttpStatusCode.Forbidden, review.StatusCode);
        var html = await review.ReadHtmlAsync();
        Assert.Contains("You can't review Nina yet", html);
        Assert.Contains("You can leave a review after Nina replies to a message from you.", html);
        Assert.Contains("You can review Nina after they reply to a message from you.", await client.GetHtmlAsync($"/accountants/{profile}"));

        var profileHtml = await client.GetHtmlAsync($"/accountants/{profile}");
        var forged = await client.PostAsync($"/accountants/{profile}/review", Form(Token(profileHtml),
            ("_handler", "review"), ("Form.Stars", "1"), ("Form.Text", "Forged"), ("command", "save")));

        // No review form is drawn for this client, so Blazor refuses the post before the page runs; the service would too.
        Assert.Equal(HttpStatusCode.BadRequest, forged.StatusCode);
        await using var db = app.Schema.CreateContext();
        Assert.False(await db.Reviews.AnyAsync(r => r.AccountantId == profile));
    }

    [DatabaseFact]
    public async Task A_client_posts_updates_and_deletes_a_review_and_the_profile_and_card_show_the_rating()
    {
        var (profile, client) = await RepliedAsync("Olive Reyes");

        var form = await client.GetHtmlAsync($"/accountants/{profile}/review");
        Assert.Contains("How was working with <strong>Nina Ocampo</strong>?", form);
        Assert.Contains("Shown publicly under the name “Olive R.”.", form);
        Assert.Contains(">Post review</button>", form);
        Assert.DoesNotContain("Delete review", form);

        var posted = await PostReviewAsync(client, profile, form, ("Form.Stars", "4"), ("Form.Text", "Clear monthly reports."));
        Assert.Equal($"/accountants/{profile}?saved=review-posted", Target(posted)?.Split('#')[0]);

        var profileHtml = await client.GetHtmlAsync($"/accountants/{profile}?saved=review-posted");
        Assert.Contains("Your review was posted.", profileHtml);
        Assert.Contains("Olive R. (you)", profileHtml);
        Assert.Contains("Clear monthly reports.", profileHtml);
        Assert.Contains("<strong>4.0</strong> average from 1 review", profileHtml);
        Assert.Contains("aria-label=\"4 out of 5 stars\"", profileHtml);
        Assert.Contains(">Edit your review</a>", profileHtml);
        Assert.DoesNotContain("Olive Reyes", ReviewsSection(profileHtml)); // Only the short name is public (R11, D21).
        var card = await app.Factory.CreateHttpsClient().GetHtmlAsync("/?q=Nina+Ocampo");
        Assert.Contains("<strong>4.0</strong><span class=\"muted\">(1 review)</span>", card);

        var edit = await client.GetHtmlAsync($"/accountants/{profile}/review");
        Assert.Contains("value=\"4\" checked", edit);
        Assert.Contains("Clear monthly reports.</textarea>", edit);
        Assert.Contains(">Update review</button>", edit);
        var updated = await PostReviewAsync(client, profile, edit, ("Form.Stars", "5"), ("Form.Text", "Even better."));
        Assert.StartsWith($"/accountants/{profile}?saved=review-updated", Target(updated));
        await using (var db = app.Schema.CreateContext())
        {
            var stored = await db.Reviews.SingleAsync(r => r.AccountantId == profile);
            Assert.Equal((5, "Even better."), (stored.Stars, stored.Text));
        }

        var deleted = await PostReviewAsync(client, profile, await client.GetHtmlAsync($"/accountants/{profile}/review"), ("command", "delete"));
        Assert.StartsWith($"/accountants/{profile}?saved=review-deleted", Target(deleted));
        await using (var db = app.Schema.CreateContext())
        {
            Assert.False(await db.Reviews.AnyAsync(r => r.AccountantId == profile));
        }
    }

    [DatabaseFact]
    public async Task A_review_without_stars_shows_the_form_again_with_the_text_kept()
    {
        var (profile, client) = await RepliedAsync("Olive Reyes");
        var form = await client.GetHtmlAsync($"/accountants/{profile}/review");

        var response = await PostReviewAsync(client, profile, form, ("Form.Text", "I forgot the stars."));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var html = await response.ReadHtmlAsync();
        Assert.Contains("Choose a rating from 1 to 5 stars.", html);
        Assert.Contains("I forgot the stars.</textarea>", html);
    }

    [DatabaseFact]
    public async Task The_accountant_replies_once_and_edits_it_and_nobody_else_can_reply()
    {
        var (profile, client, accountant) = await RepliedWithAccountantAsync("Olive Reyes");
        await PostReviewAsync(client, profile, await client.GetHtmlAsync($"/accountants/{profile}/review"), ("Form.Stars", "5"), ("Form.Text", "Great."));
        Guid reviewId;
        await using (var db = app.Schema.CreateContext())
        {
            reviewId = (await db.Reviews.SingleAsync(r => r.AccountantId == profile)).Id;
        }

        var own = await accountant.GetHtmlAsync($"/accountants/{profile}");
        Assert.Contains($"href=\"/accountants/{profile}?reply={reviewId:D}#reply-text\"", own);
        Assert.DoesNotContain("Write a review", own);
        Assert.DoesNotContain("Only clients can write reviews.", own); // The owner sees Reply, not the reason.
        Assert.DoesNotContain($"?reply={reviewId:D}", await client.GetHtmlAsync($"/accountants/{profile}"));

        var editor = await accountant.GetHtmlAsync($"/accountants/{profile}?reply={reviewId:D}");
        Assert.Contains("Your public reply", editor);
        var empty = await PostReplyAsync(accountant, profile, editor, reviewId, "  ");
        Assert.Equal(HttpStatusCode.UnprocessableEntity, empty.StatusCode);
        Assert.Contains("Write a reply first.", await empty.ReadHtmlAsync());

        var posted = await PostReplyAsync(accountant, profile, editor, reviewId, "Thank you, Olive.");
        Assert.Equal($"/accountants/{profile}?saved=reply", Target(posted)); // Then #review-…, which the client keeps.
        var shown = await client.GetHtmlAsync($"/accountants/{profile}");
        Assert.Contains("Reply from Nina", shown);
        Assert.Contains("Thank you, Olive.", shown);

        var again = await accountant.GetHtmlAsync($"/accountants/{profile}?reply={reviewId:D}");
        Assert.Contains("Thank you, Olive.</textarea>", again); // Editing starts from the saved reply.
        await PostReplyAsync(accountant, profile, again, reviewId, "Thank you, Olive. Glad it helped.");

        // The client's page draws no reply form, so the post is refused before the page runs; ReplyAsync checks the owner too.
        var forged = await PostReplyAsync(client, profile, shown, reviewId, "I'm not the accountant");
        Assert.Equal(HttpStatusCode.BadRequest, forged.StatusCode);
        await using (var db = app.Schema.CreateContext())
        {
            Assert.Equal("Thank you, Olive. Glad it helped.", (await db.Reviews.SingleAsync(r => r.Id == reviewId)).ReplyText);
        }
    }

    [DatabaseFact]
    public async Task An_unknown_accountant_has_no_review_page()
    {
        var id = AccountantId.New();
        var response = await app.Factory.CreateHttpsClient().GetAsync($"/accountants/{id}/review");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var html = await response.ReadHtmlAsync();
        Assert.Contains("This accountant is no longer available", html);
        // 2026-11-24: inside the "Write a review" panel under "Back to profile", as in the prototype's d-review.
        Assert.Contains($"""<nav class="crumbs" aria-label="Breadcrumb"><a href="/accountants/{id}">Back to profile</a></nav>""", html);
        Assert.Contains("""<h1 id="page-title" tabindex="-1">Write a review</h1>""", html);
    }

    [DatabaseFact]
    public async Task The_directory_offers_highest_rated_and_its_links_and_search_keep_the_choice()
    {
        var (_, accountantId) = await SignedInAsync(AccountRole.Accountant, "Nina Ocampo");
        await ListProfileAsync(accountantId, "Nina Ocampo");
        var visitor = app.Factory.CreateHttpsClient();

        var byName = await visitor.GetHtmlAsync("/");
        var byRating = await visitor.GetHtmlAsync("/?q=Nina&sort=rating");

        Assert.Contains("<a class=\"sortlink\" href=\"/\" aria-current=\"true\">Name (A–Z)</a>", byName);
        Assert.Contains("<a class=\"sortlink\" href=\"/?sort=rating\">Highest rated</a>", byName);
        Assert.Contains("<a class=\"sortlink\" href=\"/?q=Nina&sort=rating\" aria-current=\"true\">Highest rated</a>", byRating);
        Assert.Contains("<a class=\"sortlink\" href=\"/?q=Nina\">Name (A–Z)</a>", byRating); // Sort links keep the search.
        Assert.Contains("<input type=\"hidden\" name=\"sort\" value=\"rating\" />", byRating); // A new search keeps the order.
    }

    private static string ReviewsSection(string html) => html[html.IndexOf("id=\"reviews-heading\"", StringComparison.Ordinal)..];

    private static Task<HttpResponseMessage> PostReviewAsync(HttpClient client, AccountantId profile, string formHtml, params (string, string)[] fields)
    {
        var values = HiddenFields(formHtml, "Write a review</h1>");
        values.TryAdd("command", "save");
        foreach (var (name, value) in fields)
        {
            values[name] = value;
        }

        return client.PostAsync($"/accountants/{profile}/review", new FormUrlEncodedContent(values));
    }

    private static Task<HttpResponseMessage> PostReplyAsync(HttpClient client, AccountantId profile, string html, Guid reviewId, string text)
    {
        var values = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = Token(html),
            ["_handler"] = "reply",
            ["Reply.ReviewId"] = reviewId.ToString("D"),
            ["Reply.Text"] = text,
        };
        return client.PostAsync($"/accountants/{profile}?reply={reviewId:D}", new FormUrlEncodedContent(values));
    }

    private async Task<(AccountantId Profile, HttpClient Client)> RepliedAsync(string clientName)
    {
        var (profile, client, _) = await RepliedWithAccountantAsync(clientName);
        return (profile, client);
    }

    /// <summary>A listed accountant who has replied to a client's message, so the client may review (BR09).</summary>
    private async Task<(AccountantId Profile, HttpClient Client, HttpClient Accountant)> RepliedWithAccountantAsync(string clientName)
    {
        var (accountant, accountantId) = await SignedInAsync(AccountRole.Accountant, "Nina Ocampo");
        var profile = await ListProfileAsync(accountantId, "Nina Ocampo");
        var (client, clientId) = await SignedInAsync(AccountRole.Client, clientName);
        var conversation = await SendAsync(new ChatViewer(clientId, AccountRole.Client), profile, null, "Hi! Can you help?");
        await SendAsync(new ChatViewer(accountantId, AccountRole.Accountant), null, conversation, "Yes, happy to.");
        return (profile, client, accountant);
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
