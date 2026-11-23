using System.Net.Http.Headers;
using Microsoft.Extensions.Caching.Memory;
using TaxBuddy.Web.Application.Pictures;

namespace TaxBuddy.Web.Data;

/// <summary>
/// Profile pictures in a private Supabase Storage bucket, through Storage's REST API (ADR 011). The secret key is
/// read from configuration on the server and never sent to a browser. Reads are cached in memory: an object never
/// changes, because every upload gets a new name.
/// </summary>
public sealed class SupabasePictureStorage(HttpClient http, IConfiguration configuration, PictureCache cache) : IPictureStorage
{
    public const string DefaultBucket = "profile-pictures";

    public static bool IsConfigured(IConfiguration configuration) => SupabaseStorageApi.IsConfigured(configuration);

    public bool IsAvailable => true;

    private string Bucket => configuration["Supabase:PictureBucket"] is { Length: > 0 } bucket ? bucket : DefaultBucket;

    public async Task PutAsync(string objectName, byte[] jpeg, CancellationToken cancellationToken)
    {
        using var request = Request(HttpMethod.Post, $"storage/v1/object/{Bucket}/{objectName}");
        request.Content = new ByteArrayContent(jpeg);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        request.Headers.Add("cache-control", "max-age=31536000");
        await SendAsync(request, cancellationToken, okWhenMissing: false);
        cache.Set(objectName, jpeg);
    }

    public async Task<byte[]?> GetAsync(string objectName, CancellationToken cancellationToken)
    {
        if (cache.TryGet(objectName, out var cached))
        {
            return cached;
        }

        using var request = Request(HttpMethod.Get, $"storage/v1/object/authenticated/{Bucket}/{objectName}");
        using var response = await SendAsync(request, cancellationToken, okWhenMissing: true);
        if (response is null)
        {
            return null;
        }

        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        cache.Set(objectName, bytes);
        return bytes;
    }

    public async Task DeleteAsync(string objectName, CancellationToken cancellationToken)
    {
        cache.Remove(objectName);
        using var request = Request(HttpMethod.Delete, $"storage/v1/object/{Bucket}/{objectName}");
        using var _ = await SendAsync(request, cancellationToken, okWhenMissing: true);
    }

    private HttpRequestMessage Request(HttpMethod method, string path) => SupabaseStorageApi.Request(configuration, method, path);

    private Task<HttpResponseMessage?> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken, bool okWhenMissing) =>
        SupabaseStorageApi.SendAsync(http, request, okWhenMissing, (message, inner) => new PictureStorageException(message, inner), cancellationToken);
}

/// <summary>Used when Supabase Storage isn't configured: uploads are switched off and avatars show initials (PP07).</summary>
public sealed class NoPictureStorage : IPictureStorage
{
    public bool IsAvailable => false;

    public Task PutAsync(string objectName, byte[] jpeg, CancellationToken cancellationToken) =>
        throw new PictureStorageException("Picture storage isn't configured.");

    public Task<byte[]?> GetAsync(string objectName, CancellationToken cancellationToken) => Task.FromResult<byte[]?>(null);

    public Task DeleteAsync(string objectName, CancellationToken cancellationToken) => Task.CompletedTask;
}

/// <summary>A bounded in-memory cache of picture bytes (about 64 MB), shared by all requests.</summary>
public sealed class PictureCache : IDisposable
{
    private readonly MemoryCache cache = new(new MemoryCacheOptions { SizeLimit = 64L * 1024 * 1024 });

    public bool TryGet(string objectName, out byte[]? bytes) => cache.TryGetValue(objectName, out bytes);

    public void Set(string objectName, byte[] bytes) =>
        cache.Set(objectName, bytes, new MemoryCacheEntryOptions { Size = bytes.Length, SlidingExpiration = TimeSpan.FromHours(6) });

    public void Remove(string objectName) => cache.Remove(objectName);

    public void Dispose() => cache.Dispose();
}
