using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TaxBuddy.TestSupport;
using TaxBuddy.Web.Account;
using TaxBuddy.Web.Domain;
using TaxBuddy.Web.Tests.Support;
using static TaxBuddy.Web.Tests.Support.FormPosts;

namespace TaxBuddy.Web.Tests;

/// <summary>F06 profile pictures through the whole app (PP01, PP03–PP07), with an in-memory store for Supabase.</summary>
public sealed partial class ProfilePictureTests(EmptyAppFixture app) : IClassFixture<EmptyAppFixture>
{
    [DatabaseFact]
    public async Task An_accountant_uploads_a_picture_and_it_shows_everywhere_and_publicly()
    {
        var (client, accountId) = await SignedInAsync(AccountRole.Accountant, "Nina Ocampo");
        await ListProfileAsync(accountId, "Nina Ocampo");

        var upload = await UploadAsync(client, TestImages.Png());

        Assert.Equal(PictureEndpoints.AccountAddress("saved"), Target(upload));
        var account = await (await client.GetAsync("/account?picture=saved")).ReadHtmlAsync();
        Assert.Contains("Picture updated.", account);
        var address = PictureAddress().Match(account).Groups[1].Value;
        Assert.StartsWith($"/pictures/{accountId:D}?v=", address);
        Assert.Contains($"src=\"{address}\"", await (await client.GetAsync("/terms")).ReadHtmlAsync()); // header

        var visitor = app.Factory.CreateHttpsClient();
        var directory = await (await visitor.GetAsync("/?q=Nina")).ReadHtmlAsync();
        Assert.Contains($"src=\"{address}\"", directory);
        var picture = await visitor.GetAsync(address);
        Assert.Equal(HttpStatusCode.OK, picture.StatusCode);
        Assert.Equal("image/jpeg", picture.Content.Headers.ContentType?.MediaType);
        Assert.Equal("public, max-age=31536000, immutable", picture.Headers.CacheControl?.ToString());
        Assert.Equal((400, 400), TestImages.SizeOf(await picture.Content.ReadAsByteArrayAsync()));
    }

    [DatabaseFact]
    public async Task A_clients_picture_is_shown_to_that_client_only()
    {
        var (owner, accountId) = await SignedInAsync(AccountRole.Client, "Olive Reyes");
        await UploadAsync(owner, TestImages.Jpeg());
        var address = $"/pictures/{accountId:D}";

        var own = await owner.GetAsync(address);
        Assert.Equal(HttpStatusCode.OK, own.StatusCode);
        Assert.True(own.Headers.CacheControl is { Private: true, Public: false } cache && cache.MaxAge == TimeSpan.FromHours(1));

        Assert.Equal(HttpStatusCode.NotFound, (await app.Factory.CreateHttpsClient().GetAsync(address)).StatusCode);
        var (other, _) = await SignedInAsync(AccountRole.Client, "Paolo Tan");
        Assert.Equal(HttpStatusCode.NotFound, (await other.GetAsync(address)).StatusCode);
        Assert.Contains("Only you and the accountants you chat with see it.", await (await owner.GetAsync("/account")).ReadHtmlAsync());
    }

    [DatabaseFact]
    public async Task Replacing_and_removing_a_picture_leaves_no_old_files_and_brings_back_the_initials()
    {
        var (client, accountId) = await SignedInAsync(AccountRole.Client, "Rosa Lim");
        await UploadAsync(client, TestImages.Png());
        await UploadAsync(client, TestImages.Jpeg());
        Assert.Single(Objects(accountId));

        var page = await (await client.GetAsync("/account")).ReadHtmlAsync();
        var removed = await client.PostAsync(PictureEndpoints.RemovePath, Form(Token(page)));

        Assert.Equal(PictureEndpoints.AccountAddress("removed"), Target(removed));
        Assert.Empty(Objects(accountId));
        var after = await (await client.GetAsync("/account?picture=removed")).ReadHtmlAsync();
        Assert.Contains("Picture removed. Your initials are shown instead.", after);
        Assert.DoesNotMatch(PictureAddress(), after);
        Assert.Contains(">RL</span>", after.Replace("\n", "").Replace(" ", "")); // initials avatar
        await using var db = app.Schema.CreateContext();
        Assert.Null((await db.Users.SingleAsync(u => u.Id == accountId)).PictureObject);
    }

    [DatabaseTheory]
    [InlineData("not-an-image", "That file isn't a JPEG, PNG or WebP photo.")]
    [InlineData("too-many-pixels", "That photo has too many pixels to process.")]
    [InlineData("no-file", "Choose a photo first.")]
    public async Task Unusable_files_are_refused_with_a_message_and_store_nothing(string result, string message)
    {
        var (client, accountId) = await SignedInAsync(AccountRole.Client, "Sam Cruz");
        var bytes = result switch
        {
            "not-an-image" => TestImages.NotAnImage(),
            "too-many-pixels" => TestImages.PngClaiming(10_000, 5_000),
            _ => [],
        };

        var upload = await UploadAsync(client, bytes);

        Assert.Equal(PictureEndpoints.AccountAddress(result), Target(upload));
        Assert.Contains(message, await (await client.GetAsync($"/account?picture={result}")).ReadHtmlAsync());
        Assert.Empty(Objects(accountId));
    }

    [DatabaseFact]
    public async Task A_file_over_5_MB_is_refused_before_it_is_read()
    {
        var (client, accountId) = await SignedInAsync(AccountRole.Client, "Tess Go");

        var upload = await UploadAsync(client, new byte[6 * 1024 * 1024]);

        Assert.Equal(PictureEndpoints.AccountAddress("too-large"), Target(upload));
        Assert.Empty(Objects(accountId));
    }

    [DatabaseFact]
    public async Task A_forged_or_signed_out_post_changes_nothing()
    {
        var (client, accountId) = await SignedInAsync(AccountRole.Client, "Uma Dy");

        var noToken = new MultipartFormDataContent { { new ByteArrayContent(TestImages.Png()), "picture", "p.png" } };
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync(PictureEndpoints.UploadPath, noToken)).StatusCode);

        var signedOut = await app.Factory.CreateHttpsClient().PostAsync(PictureEndpoints.UploadPath,
            new MultipartFormDataContent { { new ByteArrayContent(TestImages.Png()), "picture", "p.png" } });
        Assert.StartsWith("/signin", Target(signedOut));
        Assert.Empty(Objects(accountId));
    }

    [DatabaseFact]
    public async Task A_storage_failure_keeps_the_current_picture()
    {
        var (client, accountId) = await SignedInAsync(AccountRole.Client, "Vic Sy");
        await UploadAsync(client, TestImages.Png());
        var before = Objects(accountId).Single();

        app.Factory.Pictures.FailWrites = true;
        try
        {
            var upload = await UploadAsync(client, TestImages.Jpeg());
            Assert.Equal(PictureEndpoints.AccountAddress("failed"), Target(upload));
        }
        finally
        {
            app.Factory.Pictures.FailWrites = false;
        }

        Assert.Equal(new[] { before }, Objects(accountId));
        Assert.Contains("We couldn't save your picture.", await (await client.GetAsync("/account?picture=failed")).ReadHtmlAsync());
    }

    [DatabaseFact]
    public async Task Without_storage_the_account_page_says_upload_is_not_set_up()
    {
        await using var factory = new TaxBuddyAppFactory(app.Schema.ConnectionString, withPictureStorage: false);
        var client = factory.CreateHttpsClient();
        await TestAccounts.RegisterAsync(client, AccountRole.Client, "Wes Ong", $"{Guid.NewGuid():N}@example.test");

        var page = await (await client.GetAsync("/account")).ReadHtmlAsync();

        Assert.Contains("Picture upload isn't set up on this site yet.", page);
        Assert.DoesNotContain(PictureEndpoints.UploadPath, page);
    }

    private async Task<(HttpClient Client, Guid AccountId)> SignedInAsync(AccountRole role, string name)
    {
        var email = $"{Guid.NewGuid():N}@example.test";
        var client = app.Factory.CreateHttpsClient();
        await TestAccounts.RegisterAsync(client, role, name, email);
        await using var db = app.Schema.CreateContext();
        return (client, (await db.Users.SingleAsync(u => u.Email == email)).Id);
    }

    /// <summary>Posts the Account page's picture form, as a browser does: multipart with its antiforgery token.</summary>
    private static async Task<HttpResponseMessage> UploadAsync(HttpClient client, byte[] bytes)
    {
        var page = await (await client.GetAsync("/account")).ReadHtmlAsync();
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        var form = new MultipartFormDataContent
        {
            { new StringContent(Token(page)), "__RequestVerificationToken" },
        };
        if (bytes.Length > 0)
        {
            form.Add(file, "picture", "photo.png");
        }

        return await client.PostAsync(PictureEndpoints.UploadPath, form);
    }

    /// <summary>A listed profile owned by the account, written directly: the editor has its own tests.</summary>
    private async Task ListProfileAsync(Guid owner, string name)
    {
        var profile = AccountantProfile.CreateFor(owner, name, "nina@example.test");
        profile.Edit(name, "Certified Public Accountant", "Taguig City", "Synthetic summary.", "nina@example.test", null, null, true);
        profile.ReplaceServices([("Bookkeeping", "")]);
        profile.MarkSaved();
        await using var db = app.Schema.CreateContext();
        db.AccountantProfiles.Add(profile);
        await db.SaveChangesAsync();
    }

    private List<string> Objects(Guid accountId) =>
        app.Factory.Pictures.Objects.Keys.Where(k => k.StartsWith($"users/{accountId:N}/", StringComparison.Ordinal)).ToList();

    [GeneratedRegex("""<img src="(/pictures/[0-9a-f-]+\?v=[0-9a-f]+)" """)]
    private static partial Regex PictureAddress();
}
