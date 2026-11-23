using System.Net.Http.Headers;
using TaxBuddy.Web.Application.Work;

namespace TaxBuddy.Web.Data;

/// <summary>
/// Service documents in their own private Supabase Storage bucket (R23; D28 decided 2026-11-23), reached only from the
/// server with the same secret key as pictures. Nothing is cached: a document is private to two people.
/// </summary>
public sealed class SupabaseDocumentStorage(HttpClient http, IConfiguration configuration) : IDocumentStorage
{
    public const string DefaultBucket = "service-documents";

    public static bool IsConfigured(IConfiguration configuration) => SupabaseStorageApi.IsConfigured(configuration);

    public bool IsAvailable => true;

    private string Bucket => configuration["Supabase:DocumentBucket"] is { Length: > 0 } bucket ? bucket : DefaultBucket;

    public async Task PutAsync(string objectName, byte[] content, string contentType, CancellationToken cancellationToken)
    {
        using var request = SupabaseStorageApi.Request(configuration, HttpMethod.Post, $"storage/v1/object/{Bucket}/{objectName}");
        request.Content = new ByteArrayContent(content);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        request.Headers.Add("cache-control", "no-store");
        using var _ = await SendAsync(request, cancellationToken, okWhenMissing: false);
    }

    public async Task<byte[]?> GetAsync(string objectName, CancellationToken cancellationToken)
    {
        using var request = SupabaseStorageApi.Request(configuration, HttpMethod.Get, $"storage/v1/object/authenticated/{Bucket}/{objectName}");
        using var response = await SendAsync(request, cancellationToken, okWhenMissing: true);
        return response is null ? null : await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    public async Task DeleteAsync(string objectName, CancellationToken cancellationToken)
    {
        using var request = SupabaseStorageApi.Request(configuration, HttpMethod.Delete, $"storage/v1/object/{Bucket}/{objectName}");
        using var _ = await SendAsync(request, cancellationToken, okWhenMissing: true);
    }

    private Task<HttpResponseMessage?> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken, bool okWhenMissing) =>
        SupabaseStorageApi.SendAsync(http, request, okWhenMissing, (message, inner) => new DocumentStorageException(message, inner), cancellationToken);
}

/// <summary>Used when Supabase Storage isn't configured: documents can't be added, and the page says so.</summary>
public sealed class NoDocumentStorage : IDocumentStorage
{
    public bool IsAvailable => false;

    public Task PutAsync(string objectName, byte[] content, string contentType, CancellationToken cancellationToken) =>
        throw new DocumentStorageException("Document storage isn't configured.");

    public Task<byte[]?> GetAsync(string objectName, CancellationToken cancellationToken) => Task.FromResult<byte[]?>(null);

    public Task DeleteAsync(string objectName, CancellationToken cancellationToken) => Task.CompletedTask;
}
