using System.Net;
using System.Text.RegularExpressions;
using TaxBuddy.Web.Account;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Web.Tests.Support;

/// <summary>Posting the site's static forms as a browser would: antiforgery token, Blazor's form name, hidden fields.</summary>
public static partial class FormPosts
{
    /// <summary>A redirect's path and query: endpoints send relative addresses, Blazor pages absolute ones.</summary>
    public static string? Target(HttpResponseMessage response) =>
        response.Headers.Location is { } location ? (location.IsAbsoluteUri ? location.PathAndQuery : location.OriginalString) : null;

    /// <summary>The cookies a response sets, as <c>name=value</c> pairs.</summary>
    public static IEnumerable<string> Cookies(HttpResponseMessage response) =>
        response.Headers.TryGetValues("Set-Cookie", out var values) ? values.Select(v => v[..v.IndexOf(';')]) : [];

    /// <summary>The first antiforgery token on the page (every form's token is valid for the same browser).</summary>
    public static string Token(string html) =>
        TokenPattern().Match(html) is { Success: true } m ? m.Groups[1].Value : throw new InvalidOperationException("No antiforgery token on the page.");

    public static FormUrlEncodedContent Form(string token, params (string Name, string Value)[] fields) =>
        new(new Dictionary<string, string>(fields.Select(f => KeyValuePair.Create(f.Name, f.Value)))
        {
            ["__RequestVerificationToken"] = token,
        });

    /// <summary>
    /// The hidden fields of the first form after <paramref name="marker"/> on the page: antiforgery token, Blazor's form
    /// name and the page's own hidden values. The header's theme form comes earlier, so the marker skips it.
    /// </summary>
    public static Dictionary<string, string> HiddenFields(string html, string marker)
    {
        var start = html.IndexOf(marker, StringComparison.Ordinal);
        Assert.True(start >= 0, $"'{marker}' is not on the page.");
        var form = html[html.IndexOf("""<form method="post" """, start, StringComparison.Ordinal)..];
        form = form[..form.IndexOf("</form>", StringComparison.Ordinal)];
        return HiddenPattern().Matches(form).ToDictionary(m => m.Groups[1].Value, m => m.Groups[2].Value);
    }

    [GeneratedRegex("""name="__RequestVerificationToken" value="([^"]+)" """)]
    private static partial Regex TokenPattern();

    [GeneratedRegex("""<input type="hidden" name="([^"]+)" value="([^"]*)" """)]
    private static partial Regex HiddenPattern();
}

/// <summary>Accounts for tests, created through the real registration page with the test sign-in (F04).</summary>
public static class TestAccounts
{
    /// <summary>
    /// Registers a new account with <paramref name="role"/>; the client is signed in afterwards. Returns the session
    /// cookie as a <c>name=value</c> pair, for tests that reuse the session on another host.
    /// </summary>
    public static async Task<string> RegisterAsync(HttpClient client, AccountRole role, string name, string email)
    {
        var key = Guid.NewGuid().ToString("N");
        Assert.Equal(HttpStatusCode.NoContent, (await client.GetAsync(TestExternalSignIn.Address(key, email, name))).StatusCode);
        await client.GetAsync(AccountEndpoints.CallbackPath + "?returnUrl=%2Fterms");
        var page = await (await client.GetAsync(AccountEndpoints.RegisterPageAddress("/terms"))).ReadHtmlAsync();

        var fields = FormPosts.HiddenFields(page, "How will you use Tax Buddy?");
        fields["Form.Role"] = AccountRoles.ValueOf(role);
        fields["Form.Agree"] = "true";
        var registered = await client.PostAsync(AccountEndpoints.RegisterPath, new FormUrlEncodedContent(fields));
        Assert.Equal("/terms", FormPosts.Target(registered));
        return FormPosts.Cookies(registered).Single(c => c.StartsWith(".AspNetCore.Identity.Application=", StringComparison.Ordinal));
    }
}
