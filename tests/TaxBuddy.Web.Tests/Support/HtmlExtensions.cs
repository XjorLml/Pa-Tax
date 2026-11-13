using System.Net;

namespace TaxBuddy.Web.Tests.Support;

/// <summary>
/// Blazor writes characters outside basic Latin as HTML entities (for example "·" as "&amp;#xB7;"). Tests read the
/// decoded page so they check what a visitor sees.
/// </summary>
public static class HtmlExtensions
{
    public static async Task<string> ReadHtmlAsync(this HttpResponseMessage response) =>
        WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

    public static async Task<string> GetHtmlAsync(this HttpClient client, string path) =>
        WebUtility.HtmlDecode(await client.GetStringAsync(path));
}
