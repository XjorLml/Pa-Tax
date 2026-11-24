using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TaxBuddy.TestSupport;
using TaxBuddy.Web.Account;
using TaxBuddy.Web.Application.Chat;
using TaxBuddy.Web.Application.Reviews;
using TaxBuddy.Web.Domain;
using TaxBuddy.Web.Tests.Support;
using static TaxBuddy.Web.Tests.Support.FormPosts;

namespace TaxBuddy.Web.Tests;

/// <summary>
/// F12 account deletion through the whole app (AD01–AD11): the two static steps, what is removed and what stays for the
/// other person (D07), the picture, a database failure, and the security check that ends other sessions.
/// </summary>
public sealed class AccountDeletionTests(EmptyAppFixture app) : IClassFixture<EmptyAppFixture>
{
    private const string DeletePage = "/account/delete";
    private const string ConfirmPage = "/account/delete?step=confirm";

    [DatabaseFact]
    public async Task Account_offers_deletion_and_the_first_step_says_what_goes_for_each_role()
    {
        var client = await RegisterAsync(AccountRole.Client, "Olive Reyes");
        var accountant = await RegisterAsync(AccountRole.Accountant, "Nina Ocampo");

        Assert.Contains("""<a class="mi danger" href="/account/delete">""", await client.Client.GetHtmlAsync("/account"));
        var clientPage = await client.Client.GetHtmlAsync(DeletePage);
        Assert.Contains("Delete your account?", clientPage);
        Assert.Contains("Your Tax Buddy sign-in. Your Google account itself is not affected.", clientPage);
        Assert.Contains("Your reviews. The accountants' ratings are recalculated.", clientPage);
        Assert.Contains("Your chats stay with the people you talked to, shown as from “Deleted user”.", clientPage);
        Assert.Contains("""<a class="btn btn-danger" href="/account/delete?step=confirm">Continue</a>""", clientPage);
        Assert.Contains("""<a class="btn" href="/account">Cancel</a>""", clientPage);
        // Prototype deleteHTML; the email clause joined on 2026-11-24, since R08 is built (F14).
        Assert.Contains("<li>Your profile picture and email notifications for this account.</li>", clientPage);

        var accountantPage = await accountant.Client.GetHtmlAsync(DeletePage);
        Assert.Contains("Your public profile, with its reviews and your replies. It leaves the directory immediately.", accountantPage);

        var confirm = await client.Client.GetHtmlAsync(ConfirmPage);
        Assert.Contains("Are you sure?", confirm);
        Assert.Contains("Deleting your account can't be undone.", confirm);
        Assert.Contains(""">Delete my account</button>""", confirm);
    }

    [DatabaseFact]
    public async Task Deleting_a_client_removes_their_review_and_leaves_the_accountant_a_read_only_chat()
    {
        var accountant = await RegisterAsync(AccountRole.Accountant, "Nina Ocampo");
        var profile = await ListProfileAsync(accountant.Id, "Nina Ocampo");
        var client = await RegisterAsync(AccountRole.Client, "Olive Reyes");
        var conversation = await SendAsync(new ChatViewer(client.Id, AccountRole.Client), profile, null, "Hi! Can you help?");
        await SendAsync(new ChatViewer(accountant.Id, AccountRole.Accountant), null, conversation, "Yes.");
        await ReviewAsync(client.Id, profile);
        Assert.Contains("1 review", await client.Client.GetHtmlAsync($"/accountants/{profile}"));

        var deleted = await DeleteAsync(client.Client);

        Assert.Equal(HttpStatusCode.Redirect, deleted.StatusCode);
        Assert.Equal("/account/delete?done=1", Target(deleted));
        Assert.Contains(".AspNetCore.Identity.Application=", Cookies(deleted)); // Cleared: signed out.
        Assert.Contains("Your account was deleted", await client.Client.GetHtmlAsync("/account/delete?done=1"));

        await using (var db = app.Schema.CreateContext())
        {
            var user = await db.Users.SingleAsync(u => u.Id == client.Id);
            Assert.Equal((DeletedAccount.DisplayName, null, null, null), (user.DisplayName, user.Email, user.NormalizedEmail, user.PictureObject));
            Assert.NotNull(user.DeletedAt);
            Assert.False(await db.UserLogins.AnyAsync(l => l.UserId == client.Id));
            Assert.False(await db.Reviews.AnyAsync(r => r.ClientAccountId == client.Id));
            Assert.True(await db.Conversations.AnyAsync(c => c.Id == conversation));
            Assert.Equal(2, await db.Messages.CountAsync(m => m.ConversationId == conversation));
        }

        Assert.Contains("No reviews yet", await accountant.Client.GetHtmlAsync($"/accountants/{profile}"));
        var chat = await accountant.Client.GetHtmlAsync($"/chats/{conversation:D}");
        Assert.Contains(DeletedAccount.DisplayName, chat);
        Assert.Contains("This client deleted their account. You can still read this conversation, but you can't reply.", chat);
        Assert.DoesNotContain("data-chat-composer", chat);
        Assert.DoesNotContain(">Block</button>", chat);
        using var scope = app.Factory.Services.CreateScope();
        var reply = await scope.ServiceProvider.GetRequiredService<ChatService>()
            .SendAsync(new ChatViewer(accountant.Id, AccountRole.Accountant), conversation, null, Guid.NewGuid(), "Still there?");
        Assert.Equal(SendOutcome.Blocked, reply.Outcome);

        // Signing in again with the same Google account starts a new registration.
        var again = app.Factory.CreateHttpsClient();
        Assert.Equal(HttpStatusCode.NoContent, (await again.GetAsync(TestExternalSignIn.Address(client.Key, client.Email, client.Name))).StatusCode);
        var callback = await again.GetAsync(AccountEndpoints.CallbackPath + "?returnUrl=%2F");
        Assert.Equal(AccountEndpoints.RegisterPageAddress("/"), Target(callback));
    }

    [DatabaseFact]
    public async Task Deleting_an_accountant_removes_the_profile_and_leaves_the_client_a_read_only_chat()
    {
        // A name no other test in this schema uses, so the directory check means something.
        var accountant = await RegisterAsync(AccountRole.Accountant, "Rosa Leaving");
        var profile = await ListProfileAsync(accountant.Id, "Rosa Leaving");
        var client = await RegisterAsync(AccountRole.Client, "Olive Reyes");
        var conversation = await SendAsync(new ChatViewer(client.Id, AccountRole.Client), profile, null, "Hi! Can you help?");
        await SendAsync(new ChatViewer(accountant.Id, AccountRole.Accountant), null, conversation, "Yes.");
        await ReviewAsync(client.Id, profile);

        Assert.Equal("/account/delete?done=1", Target(await DeleteAsync(accountant.Client)));

        await using (var db = app.Schema.CreateContext())
        {
            Assert.False(await db.AccountantProfiles.AnyAsync(p => p.OwnerAccountId == accountant.Id));
            Assert.False(await db.Reviews.AnyAsync(r => r.ClientAccountId == client.Id)); // Reviews of the profile go with it.
            Assert.True(await db.Conversations.AnyAsync(c => c.Id == conversation));
        }

        var visitor = app.Factory.CreateHttpsClient();
        var oldLink = await visitor.GetAsync($"/accountants/{profile}");
        Assert.Equal(HttpStatusCode.NotFound, oldLink.StatusCode);
        Assert.Contains("This profile is no longer available", await oldLink.ReadHtmlAsync());
        Assert.DoesNotContain("Rosa Leaving", await visitor.GetHtmlAsync("/?q=Rosa"));

        var chat = await client.Client.GetHtmlAsync($"/chats/{conversation:D}");
        Assert.Contains(DeletedAccount.DisplayName, chat);
        Assert.Contains("This accountant deleted their account. You can still read this conversation, but you can't reply.", chat);
        Assert.DoesNotContain("data-chat-composer", chat);
    }

    [DatabaseFact]
    public async Task When_both_people_have_deleted_their_accounts_the_conversation_goes()
    {
        var accountant = await RegisterAsync(AccountRole.Accountant, "Nina Ocampo");
        var profile = await ListProfileAsync(accountant.Id, "Nina Ocampo");
        var client = await RegisterAsync(AccountRole.Client, "Olive Reyes");
        var conversation = await SendAsync(new ChatViewer(client.Id, AccountRole.Client), profile, null, "Hi! Can you help?");

        await DeleteAsync(client.Client);
        await using (var db = app.Schema.CreateContext())
        {
            Assert.True(await db.Conversations.AnyAsync(c => c.Id == conversation));
        }

        await DeleteAsync(accountant.Client);
        await using (var db = app.Schema.CreateContext())
        {
            Assert.False(await db.Conversations.AnyAsync(c => c.Id == conversation));
            Assert.False(await db.Messages.AnyAsync(m => m.ConversationId == conversation));
        }
    }

    [DatabaseFact]
    public async Task The_picture_is_removed_from_storage()
    {
        var client = await RegisterAsync(AccountRole.Client, "Olive Reyes");
        var objectName = $"users/{client.Id:N}/test.jpg";
        app.Factory.Pictures.Objects[objectName] = [1, 2, 3];
        await using (var db = app.Schema.CreateContext())
        {
            (await db.Users.SingleAsync(u => u.Id == client.Id)).PictureObject = objectName;
            await db.SaveChangesAsync();
        }

        Assert.Equal("/account/delete?done=1", Target(await DeleteAsync(client.Client)));

        Assert.False(app.Factory.Pictures.Objects.ContainsKey(objectName));
        Assert.Contains(app.Factory.Logs.Messages, m => m.Contains("Account deleted", StringComparison.Ordinal));
        Assert.DoesNotContain(app.Factory.Logs.Messages, m => m.Contains(client.Id.ToString(), StringComparison.OrdinalIgnoreCase)
                                                            || m.Contains(client.Email, StringComparison.OrdinalIgnoreCase));
    }

    [DatabaseFact]
    public async Task When_the_database_fails_nothing_is_removed_and_you_stay_signed_in()
    {
        // Sign in on the working database, then use the same session on a host whose database is unreachable.
        var session = await TestAccounts.RegisterAsync(app.Factory.CreateHttpsClient(), AccountRole.Client, "Lea Cruz", $"{Guid.NewGuid():N}@example.test");
        await using var broken = new TaxBuddyAppFactory(TaxBuddyAppFactory.UnreachableDatabase);
        var client = broken.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost"),
            HandleCookies = false,
        });

        var confirm = await client.SendAsync(WithCookies(HttpMethod.Get, ConfirmPage, session));
        var antiforgery = Cookies(confirm).Single(c => c.StartsWith(".AspNetCore.Antiforgery.", StringComparison.Ordinal));
        var post = WithCookies(HttpMethod.Post, ConfirmPage, session, antiforgery);
        post.Content = new FormUrlEncodedContent(HiddenFields(await confirm.ReadHtmlAsync(), "Are you sure?"));
        var response = await client.SendAsync(post);
        var html = await response.ReadHtmlAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("We couldn't delete your account, so nothing was removed. You're still signed in. Please try again.", html);
        Assert.DoesNotContain(Cookies(response), c => c.StartsWith(".AspNetCore.Identity.Application=", StringComparison.Ordinal));
    }

    [DatabaseFact]
    public async Task A_visitor_is_asked_to_sign_in_and_a_forged_post_changes_nothing()
    {
        var visitor = app.Factory.CreateHttpsClient();
        var page = await visitor.GetHtmlAsync(DeletePage);
        Assert.Contains("Sign in to manage your account", page);
        Assert.Contains("""href="/signin?returnUrl=%2Faccount%2Fdelete">Sign in</a>""", page);

        var client = await RegisterAsync(AccountRole.Client, "Olive Reyes");
        var forged = await client.Client.PostAsync(ConfirmPage, new FormUrlEncodedContent(new Dictionary<string, string> { ["_handler"] = "delete-account" }));

        Assert.Equal(HttpStatusCode.BadRequest, forged.StatusCode);
        await using var db = app.Schema.CreateContext();
        Assert.Null((await db.Users.SingleAsync(u => u.Id == client.Id)).DeletedAt);
    }

    [DatabaseFact]
    public async Task Someone_who_has_not_agreed_to_the_current_terms_can_still_delete()
    {
        var client = await RegisterAsync(AccountRole.Client, "Olive Reyes");
        await using (var db = app.Schema.CreateContext())
        {
            (await db.Users.SingleAsync(u => u.Id == client.Id)).AgreedTermsVersion = "draft-2026-11-16";
            await db.SaveChangesAsync();
        }

        Assert.Equal(HttpStatusCode.NoContent, (await client.Client.GetAsync(TestExternalSignIn.Address(client.Key, client.Email, client.Name))).StatusCode);
        await client.Client.GetAsync(AccountEndpoints.CallbackPath + "?returnUrl=%2F");

        Assert.Equal(HttpStatusCode.Redirect, (await client.Client.GetAsync("/account")).StatusCode); // Agree first elsewhere (F11).
        Assert.Equal(HttpStatusCode.OK, (await client.Client.GetAsync(DeletePage)).StatusCode);
        Assert.Equal("/account/delete?done=1", Target(await DeleteAsync(client.Client)));
    }

    [Fact]
    public void Other_sessions_are_checked_against_the_account_every_minute()
    {
        using var factory = new TaxBuddyAppFactory(TaxBuddyAppFactory.UnreachableDatabase);

        var options = factory.Services.GetRequiredService<IOptions<SecurityStampValidatorOptions>>().Value;

        Assert.Equal(TimeSpan.FromMinutes(1), options.ValidationInterval);
    }

    private sealed record Person(HttpClient Client, Guid Id, string Key, string Email, string Name);

    private async Task<Person> RegisterAsync(AccountRole role, string name)
    {
        var client = app.Factory.CreateHttpsClient();
        var key = Guid.NewGuid().ToString("N");
        var email = $"{key}@example.test";
        Assert.Equal(HttpStatusCode.NoContent, (await client.GetAsync(TestExternalSignIn.Address(key, email, name))).StatusCode);
        await client.GetAsync(AccountEndpoints.CallbackPath + "?returnUrl=%2Fterms");
        var page = await client.GetHtmlAsync(AccountEndpoints.RegisterPageAddress("/terms"));
        var fields = HiddenFields(page, "How will you use Tax Buddy?");
        fields["Form.Role"] = AccountRoles.ValueOf(role);
        fields["Form.Agree"] = "true";
        Assert.Equal("/terms", Target(await client.PostAsync(AccountEndpoints.RegisterPath, new FormUrlEncodedContent(fields))));
        await using var db = app.Schema.CreateContext();
        return new Person(client, (await db.Users.SingleAsync(u => u.Email == email)).Id, key, email, name);
    }

    private static async Task<HttpResponseMessage> DeleteAsync(HttpClient client)
    {
        var confirm = await client.GetHtmlAsync(ConfirmPage);
        return await client.PostAsync(ConfirmPage, new FormUrlEncodedContent(HiddenFields(confirm, "Are you sure?")));
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

    private async Task<Guid> SendAsync(ChatViewer sender, AccountantId? newChatWith, Guid? conversation, string text)
    {
        using var scope = app.Factory.Services.CreateScope();
        var sent = await scope.ServiceProvider.GetRequiredService<ChatService>()
            .SendAsync(sender, conversation, newChatWith, Guid.NewGuid(), text);
        Assert.Equal(SendOutcome.Sent, sent.Outcome);
        return sent.ConversationId!.Value;
    }

    private async Task ReviewAsync(Guid clientId, AccountantId profile)
    {
        using var scope = app.Factory.Services.CreateScope();
        var saved = await scope.ServiceProvider.GetRequiredService<ReviewService>()
            .SaveAsync(new ChatViewer(clientId, AccountRole.Client), profile, new ReviewForm { Stars = "5", Text = "Helpful." });
        Assert.Equal(ReviewOutcome.Posted, saved.Outcome);
    }

    private static HttpRequestMessage WithCookies(HttpMethod method, string path, params string[] cookies)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Add("Cookie", string.Join("; ", cookies));
        return request;
    }
}
