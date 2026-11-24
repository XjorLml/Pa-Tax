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
/// F07 chat pages through the whole app (CH01, CH02, CH05, CH07, CH08). The live panel is prerendered, so its first
/// response already holds the list or the thread; sending through the live connection is checked in a browser.
/// </summary>
public sealed class ChatPageTests(EmptyAppFixture app) : IClassFixture<EmptyAppFixture>
{
    [DatabaseFact]
    public async Task A_visitor_is_asked_to_sign_in_for_the_chat_list_and_a_thread()
    {
        var visitor = app.Factory.CreateHttpsClient();

        var list = await visitor.GetAsync("/chats");
        var thread = await visitor.GetAsync($"/chats/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        var html = await list.ReadHtmlAsync();
        Assert.Contains("Sign in to see your chats", html);
        Assert.Contains("href=\"/signin?returnUrl=%2Fchats\"", html);
        Assert.DoesNotContain("data-chat-composer", html);
        Assert.Contains("Sign in to see this chat", await thread.ReadHtmlAsync());
    }

    [DatabaseFact]
    public async Task A_visitor_can_write_to_an_accountant_and_signs_in_to_send()
    {
        var (_, accountantId) = await SignedInAsync(AccountRole.Accountant, "Nina Ocampo");
        var profile = await ListProfileAsync(accountantId, "Nina Ocampo");

        var response = await app.Factory.CreateHttpsClient().GetAsync($"/chats/new/{profile}");
        var html = await response.ReadHtmlAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Write your message to Nina. You'll sign in before it's sent, and your text is kept.", html);
        Assert.Contains("Sign in to send", html);
        Assert.Contains("data-chat-composer", html);
        Assert.Contains("Hi! Can you help me file my taxes?", html); // Message ideas.
        // F11 LP09: the hint about sensitive documents, read with the message box.
        Assert.Contains("""<p class="composer-note" id="composer-note">""", html);
        Assert.Contains("Don't share TINs, tax returns or ID numbers in chat.", html);
        Assert.Contains("""aria-describedby="composer-note composer-status composer-count" """.Trim(), html);
    }

    [DatabaseFact]
    public async Task A_client_messages_an_accountant_who_sees_it_as_new_and_messaging_again_opens_the_same_chat()
    {
        var (accountant, accountantId) = await SignedInAsync(AccountRole.Accountant, "Nina Ocampo");
        var profile = await ListProfileAsync(accountantId, "Nina Ocampo");
        var (client, clientId) = await SignedInAsync(AccountRole.Client, "Olive Reyes");

        var compose = await (await client.GetAsync($"/chats/new/{profile}")).ReadHtmlAsync();
        Assert.Contains("Say hello to Nina and describe what you need help with.", compose);
        Assert.Contains(">Send</span>", compose);
        var conversation = await SendAsync(clientId, profile, "Hi! I need help with monthly bookkeeping.");

        var list = await (await accountant.GetAsync("/chats")).ReadHtmlAsync();
        Assert.Contains("Olive Reyes", list);
        Assert.Contains("Hi! I need help with monthly bookkeeping.", list);
        Assert.Contains("1 new", list);
        Assert.Contains("1<span class=\"sr-only\"> unread</span>", list); // The navigation count.

        var again = await client.GetAsync($"/chats/new/{profile}");
        Assert.Equal($"/chats/{conversation:D}", Target(again));

        var thread = await accountant.GetAsync($"/chats/{conversation:D}");
        var threadHtml = await thread.ReadHtmlAsync();
        Assert.Equal(HttpStatusCode.OK, thread.StatusCode);
        Assert.Contains("Hi! I need help with monthly bookkeeping.", threadHtml);
        Assert.Contains(">Today</div>", threadHtml);
        Assert.DoesNotContain("View profile", threadHtml); // A client has no public profile.
        var clientThread = await (await client.GetAsync($"/chats/{conversation:D}")).ReadHtmlAsync();
        Assert.Contains($"href=\"/accountants/{profile}\"", clientThread);
        Assert.Contains("· Sent", clientThread);
    }

    [DatabaseFact]
    public async Task A_conversation_is_not_available_to_anyone_else()
    {
        var (_, accountantId) = await SignedInAsync(AccountRole.Accountant, "Nina Ocampo");
        var profile = await ListProfileAsync(accountantId, "Nina Ocampo");
        var (_, clientId) = await SignedInAsync(AccountRole.Client, "Olive Reyes");
        var conversation = await SendAsync(clientId, profile, "Something private");
        var (outsider, _) = await SignedInAsync(AccountRole.Client, "Paolo Tan");

        foreach (var address in new[] { $"/chats/{conversation:D}", $"/chats/{Guid.NewGuid():D}" })
        {
            var response = await outsider.GetAsync(address);
            var html = await response.ReadHtmlAsync();
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("This conversation isn't available", html);
            Assert.DoesNotContain("Something private", html);
            Assert.DoesNotContain("Olive Reyes", html);
            Assert.DoesNotContain("Nina Ocampo", html);
        }

        Assert.DoesNotContain("Olive Reyes", await (await outsider.GetAsync("/chats")).ReadHtmlAsync());
    }

    [DatabaseFact]
    public async Task Accountants_cannot_start_chats_and_unlisted_profiles_cannot_be_messaged()
    {
        var (accountant, accountantId) = await SignedInAsync(AccountRole.Accountant, "Nina Ocampo");
        var profile = await ListProfileAsync(accountantId, "Nina Ocampo");
        var (client, _) = await SignedInAsync(AccountRole.Client, "Olive Reyes");

        Assert.Contains("Only clients can start chats", await (await accountant.GetAsync($"/chats/new/{profile}")).ReadHtmlAsync());
        Assert.Contains("No client messages yet", await (await accountant.GetAsync("/chats")).ReadHtmlAsync());
        Assert.Contains("No chats yet", await (await client.GetAsync("/chats")).ReadHtmlAsync());

        var unknown = await client.GetAsync($"/chats/new/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
        Assert.Contains("This accountant is no longer available", await unknown.ReadHtmlAsync());
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/chats/new/not-an-id")).StatusCode);
    }

    [DatabaseFact]
    public async Task An_accountant_is_told_above_the_list_whether_new_messages_are_emailed()
    {
        // F14 (R08), prototype convListHTML: the note joined on 2026-11-24. Clients don't get it.
        var (accountant, accountantId) = await SignedInAsync(AccountRole.Accountant, "Nina Ocampo");
        var profile = await ListProfileAsync(accountantId, "Nina Ocampo");
        var (client, clientId) = await SignedInAsync(AccountRole.Client, "Olive Reyes");
        await SendAsync(clientId, profile, "Hello");

        Assert.Contains("""<p class="muted small conv-note">New client messages are also emailed to you. <a href="/account">Change</a></p>""",
            await (await accountant.GetAsync("/chats")).ReadHtmlAsync());
        Assert.DoesNotContain("conv-note", await (await client.GetAsync("/chats")).ReadHtmlAsync());

        await using (var db = app.Schema.CreateContext())
        {
            await db.Users.Where(u => u.Id == accountantId).ExecuteUpdateAsync(s => s.SetProperty(u => u.EmailNotifications, false));
        }

        Assert.Contains("""<p class="muted small conv-note">Email notifications are off. <a href="/account">Change</a></p>""",
            await (await accountant.GetAsync("/chats")).ReadHtmlAsync());
    }

    [DatabaseFact]
    public async Task An_accountant_can_see_the_picture_of_a_client_they_chat_with()
    {
        var (accountant, accountantId) = await SignedInAsync(AccountRole.Accountant, "Nina Ocampo");
        var profile = await ListProfileAsync(accountantId, "Nina Ocampo");
        var (client, clientId) = await SignedInAsync(AccountRole.Client, "Olive Reyes");
        var (otherAccountant, _) = await SignedInAsync(AccountRole.Accountant, "Rico Santos");
        var page = await (await client.GetAsync("/account")).ReadHtmlAsync();
        var form = new MultipartFormDataContent
        {
            { new StringContent(Token(page)), "__RequestVerificationToken" },
            { new ByteArrayContent(TestImages.Png()), "picture", "photo.png" },
        };
        await client.PostAsync(TaxBuddy.Web.Account.PictureEndpoints.UploadPath, form);
        var address = $"/pictures/{clientId:D}";
        Assert.Equal(HttpStatusCode.NotFound, (await accountant.GetAsync(address)).StatusCode);

        await SendAsync(clientId, profile, "Hello");

        Assert.Equal(HttpStatusCode.OK, (await accountant.GetAsync(address)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await otherAccountant.GetAsync(address)).StatusCode);
        Assert.Contains($"src=\"{address}?v=", await (await accountant.GetAsync("/chats")).ReadHtmlAsync());
    }

    /// <summary>A client's message through the app's own chat service (the live panel's send needs a browser).</summary>
    private async Task<Guid> SendAsync(Guid clientId, AccountantId profile, string text)
    {
        using var scope = app.Factory.Services.CreateScope();
        var sent = await scope.ServiceProvider.GetRequiredService<ChatService>()
            .SendAsync(new ChatViewer(clientId, AccountRole.Client), null, profile, Guid.NewGuid(), text);
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
