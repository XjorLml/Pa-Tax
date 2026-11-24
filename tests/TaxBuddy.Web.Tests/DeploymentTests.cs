using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using TaxBuddy.Web.Account;
using TaxBuddy.Web.Services;
using TaxBuddy.Web.Tests.Support;
using static TaxBuddy.Web.Tests.Support.FormPosts;

namespace TaxBuddy.Web.Tests;

/// <summary>
/// ADR 012 and RELEASE: what the container on the OCI VM relies on in the app. The health address, the scheme forwarded
/// by Caddy reaching Google's callback address, and data-protection keys kept in the container's volume. No database.
/// </summary>
public sealed class DeploymentTests
{
    [Fact]
    public async Task The_health_address_answers_without_the_database_and_names_the_build()
    {
        await using var built = new TaxBuddyAppFactory(TaxBuddyAppFactory.UnreachableDatabase,
            settings: new Dictionary<string, string> { [DeploymentSetup.VersionSetting] = "abc1234" });
        await using var local = new TaxBuddyAppFactory(TaxBuddyAppFactory.UnreachableDatabase);

        var response = await built.CreateHttpsClient().GetAsync(DeploymentSetup.HealthPath);
        var health = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("ok", health.GetProperty("status").GetString());
        Assert.Equal("abc1234", health.GetProperty("version").GetString());
        Assert.Equal("no-store", response.Headers.CacheControl!.ToString());
        Assert.Contains("\"version\":\"dev\"", await local.CreateHttpsClient().GetStringAsync(DeploymentSetup.HealthPath));
    }

    [Fact]
    public async Task Behind_the_proxy_Google_is_given_an_https_callback()
    {
        // The container sets ASPNETCORE_FORWARDEDHEADERS_ENABLED; the request reaches the app as plain HTTP from Caddy.
        await using var proxied = new TaxBuddyAppFactory(TaxBuddyAppFactory.UnreachableDatabase,
            settings: new Dictionary<string, string> { ["ForwardedHeaders_Enabled"] = "true" });
        await using var direct = new TaxBuddyAppFactory(TaxBuddyAppFactory.UnreachableDatabase);

        Assert.Contains("redirect_uri=" + Uri.EscapeDataString("https://localhost/signin-google"), await ChallengeAsync(proxied));
        Assert.Contains("redirect_uri=" + Uri.EscapeDataString("http://localhost/signin-google"), await ChallengeAsync(direct));
    }

    [Fact]
    public async Task Keys_are_kept_in_the_configured_folder()
    {
        var folder = Directory.CreateTempSubdirectory("taxbuddy-keys-");
        try
        {
            await using (var factory = new TaxBuddyAppFactory(TaxBuddyAppFactory.UnreachableDatabase,
                             settings: new Dictionary<string, string> { [DeploymentSetup.KeysPathSetting] = folder.FullName }))
            {
                await factory.CreateHttpsClient().GetStringAsync("/signin"); // its form token needs a key
            }

            Assert.Single(folder.GetFiles("key-*.xml"));
        }
        finally
        {
            folder.Delete(recursive: true);
        }
    }

    /// <summary>Starts Google sign-in over plain HTTP as Caddy forwards it, and returns where the site sends the browser.</summary>
    private static async Task<string> ChallengeAsync(TaxBuddyAppFactory factory)
    {
        // Cookies by hand: they are Secure once the forwarded scheme is https, and the client would not send them over http.
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = false });
        client.DefaultRequestHeaders.Add("X-Forwarded-Proto", "https");
        client.DefaultRequestHeaders.Add("X-Forwarded-For", "203.0.113.5");

        var page = await client.GetAsync("/signin");
        var html = await page.ReadHtmlAsync();
        using var post = new HttpRequestMessage(HttpMethod.Post, AccountEndpoints.ExternalSignInPath)
        {
            Content = Form(Token(html), ("provider", "Google"), ("returnUrl", "/")),
        };
        post.Headers.Add("Cookie", string.Join("; ", Cookies(page)));
        var challenge = await client.SendAsync(post);

        Assert.Equal(HttpStatusCode.Redirect, challenge.StatusCode);
        return challenge.Headers.Location!.AbsoluteUri;
    }
}
