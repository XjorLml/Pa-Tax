using System.Net;
using System.Net.Http.Headers;

namespace TaxBuddy.Web.Data;

/// <summary>
/// The parts of Supabase Storage's REST API that pictures (ADR 011) and service documents (D28) share: the project
/// address, the server-only secret key, and how a missing object or a failure is reported.
/// </summary>
internal static class SupabaseStorageApi
{
    public static bool IsConfigured(IConfiguration configuration) =>
        Uri.TryCreate(configuration["Supabase:Url"], UriKind.Absolute, out _)
        && !string.IsNullOrWhiteSpace(configuration["Supabase:SecretKey"]);

    public static HttpRequestMessage Request(IConfiguration configuration, HttpMethod method, string path)
    {
        var baseUrl = new Uri(configuration["Supabase:Url"]!.TrimEnd('/') + "/");
        var key = configuration["Supabase:SecretKey"]!;
        var request = new HttpRequestMessage(method, new Uri(baseUrl, path));
        request.Headers.Add("apikey", key);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key);
        return request;
    }

    /// <summary>The response when successful; null when the object is missing and that's acceptable; otherwise <paramref name="failure"/>.</summary>
    public static async Task<HttpResponseMessage?> SendAsync(HttpClient http, HttpRequestMessage request, bool okWhenMissing,
        Func<string, Exception?, Exception> failure, CancellationToken cancellationToken)
    {
        HttpResponseMessage response;
        try
        {
            response = await http.SendAsync(request, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            throw failure("Storage could not be reached.", ex);
        }

        if (response.IsSuccessStatusCode)
        {
            return response;
        }

        // Storage reports a missing object as 404, or as 400 with a not-found body.
        var missing = response.StatusCode == HttpStatusCode.NotFound
                      || (response.StatusCode == HttpStatusCode.BadRequest
                          && (await response.Content.ReadAsStringAsync(cancellationToken)).Contains("not_found", StringComparison.OrdinalIgnoreCase));
        var status = (int)response.StatusCode;
        response.Dispose();
        if (missing && okWhenMissing)
        {
            return null;
        }

        // The status only: response bodies can echo request details.
        throw failure($"Storage answered {status}.", null);
    }
}
