using System.Net;
using System.Text.RegularExpressions;
using TaxBuddy.Web.Services;
using TaxBuddy.Web.Tests.Support;

namespace TaxBuddy.Web.Tests;

/// <summary>
/// F02 BD09 theme switch through the whole app. The not-found page needs no database, so these tests run without one.
/// </summary>
public sealed partial class ThemeSwitchTests : IDisposable
{
    private const string Page = "/no-such-page?x=1";

    private readonly TaxBuddyAppFactory factory = new(TaxBuddyAppFactory.UnreachableDatabase);

    [Fact]
    public async Task Without_a_choice_the_page_follows_the_browser_and_System_is_pressed()
    {
        var html = await ReadPageAsync(factory.CreateClient());

        Assert.Matches("""<html lang="en">""", html);
        Assert.Equal("system", PressedOption(html));
        Assert.Contains("""name="returnUrl" value="/no-such-page?x=1" """.Trim(), html);
        Assert.Contains("""role="group" aria-labelledby="theme-switch-label" """.Trim(), html);
    }

    [Theory]
    [InlineData("dark")]
    [InlineData("light")]
    public async Task Choosing_a_theme_stores_it_and_returns_to_the_same_address(string theme)
    {
        var client = factory.CreateClientWithoutRedirects();

        var response = await PostAsync(client, theme, Page);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(Page, response.Headers.Location?.OriginalString);
        var cookie = Assert.Single(response.Headers.GetValues("Set-Cookie"), c => c.StartsWith(ThemePreference.CookieName + "=", StringComparison.Ordinal));
        Assert.StartsWith($"{ThemePreference.CookieName}={theme};", cookie);
        Assert.Contains("max-age=31536000", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("path=/", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);

        var html = await ReadPageAsync(client);
        Assert.Contains($"""<html lang="en" data-theme="{theme}">""", html);
        Assert.Equal(theme, PressedOption(html));
    }

    [Fact]
    public async Task Choosing_System_removes_the_stored_choice()
    {
        var client = factory.CreateClientWithoutRedirects();
        await PostAsync(client, "dark", Page);

        var response = await PostAsync(client, "system", Page);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var cookie = Assert.Single(response.Headers.GetValues("Set-Cookie"), c => c.StartsWith(ThemePreference.CookieName + "=", StringComparison.Ordinal));
        Assert.Contains("expires=Thu, 01 Jan 1970", cookie, StringComparison.OrdinalIgnoreCase);

        var html = await ReadPageAsync(client);
        Assert.Contains("""<html lang="en">""", html);
        Assert.Equal("system", PressedOption(html));
    }

    [Fact]
    public async Task A_post_without_the_antiforgery_token_is_refused_and_stores_nothing()
    {
        var client = factory.CreateClientWithoutRedirects();

        var response = await client.PostAsync(ThemePreference.EndpointPath, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["theme"] = "dark",
            ["returnUrl"] = "/",
        }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(response.Headers.TryGetValues("Set-Cookie", out var cookies)
                     && cookies.Any(c => c.StartsWith(ThemePreference.CookieName + "=", StringComparison.Ordinal)));
    }

    [Fact]
    public async Task An_unknown_theme_is_refused()
    {
        var response = await PostAsync(factory.CreateClientWithoutRedirects(), "purple", Page);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("https://evil.example/")]
    [InlineData("//evil.example/")]
    [InlineData("/\\evil.example/")]
    [InlineData("")]
    public async Task A_return_address_off_this_site_goes_home_instead(string returnUrl)
    {
        var response = await PostAsync(factory.CreateClientWithoutRedirects(), "dark", returnUrl);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData("/", true)]
    [InlineData("/accountants/1?x=a%2Fb", true)]
    [InlineData("//evil.example", false)]
    [InlineData("/\\evil.example", false)]
    [InlineData("/a\\b", false)]
    [InlineData("/a\r\nLocation: x", false)]
    [InlineData("https://evil.example", false)]
    [InlineData("evil", false)]
    [InlineData(null, false)]
    public void Only_addresses_on_this_site_are_local(string? address, bool expected) =>
        Assert.Equal(expected, LocalAddress.IsLocal(address));

    [Fact]
    public void Both_dark_token_blocks_in_the_stylesheet_are_identical()
    {
        var css = File.ReadAllText(Path.Combine(RepositoryRoot(), "src", "TaxBuddy.Web", "Styles", "app.css"));

        var system = Declarations(css, "/* dark-tokens: system */");
        var chosen = Declarations(css, "/* dark-tokens: chosen */");

        Assert.True(system.Count >= 18, $"Expected the full dark palette, found {system.Count} declarations.");
        Assert.Equal(system, chosen);
    }

    public void Dispose() => factory.Dispose();

    private static async Task<HttpResponseMessage> PostAsync(HttpClient client, string theme, string returnUrl)
    {
        var page = await ReadPageAsync(client);
        var token = TokenPattern().Match(page);
        Assert.True(token.Success, "The theme switch has no antiforgery token.");

        return await client.PostAsync(ThemePreference.EndpointPath, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token.Groups[1].Value,
            ["theme"] = theme,
            ["returnUrl"] = returnUrl,
        }));
    }

    /// <summary>The not-found page, which answers 404 by design; read it without treating that as a failure.</summary>
    private static async Task<string> ReadPageAsync(HttpClient client)
    {
        var response = await client.GetAsync(Page);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        return await response.ReadHtmlAsync();
    }

    private static string? PressedOption(string html)
    {
        var pressed = PressedPattern().Matches(html).Select(m => m.Groups[1].Value).ToList();
        return Assert.Single(pressed);
    }

    /// <summary>The custom-property declarations after a marker comment, up to the first closing brace.</summary>
    private static List<string> Declarations(string css, string marker)
    {
        var start = css.IndexOf(marker, StringComparison.Ordinal);
        Assert.True(start >= 0, $"Marker not found: {marker}");
        var open = css.IndexOf('{', css.IndexOf(":root", start, StringComparison.Ordinal));
        var close = css.IndexOf('}', open);
        return css[(open + 1)..close]
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    private static string RepositoryRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "TaxBuddy.slnx")))
            {
                return dir.FullName;
            }
        }

        throw new InvalidOperationException("TaxBuddy.slnx not found above the test output folder.");
    }

    [GeneratedRegex("""name="__RequestVerificationToken" value="([^"]+)" """)]
    private static partial Regex TokenPattern();

    [GeneratedRegex("""<button type="submit" name="theme" value="(\w+)" class="theme-option" aria-pressed="true" """)]
    private static partial Regex PressedPattern();
}
