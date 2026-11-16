using System.Collections.Concurrent;
using TaxBuddy.Web.Application.Pictures;

namespace TaxBuddy.Web.Tests.Support;

/// <summary>Stands in for Supabase Storage in web tests (F06). Can be told to fail, to check that nothing changes.</summary>
public sealed class InMemoryPictureStorage : IPictureStorage
{
    public ConcurrentDictionary<string, byte[]> Objects { get; } = new();

    public bool FailWrites { get; set; }

    public bool IsAvailable => true;

    public Task PutAsync(string objectName, byte[] jpeg, CancellationToken cancellationToken)
    {
        if (FailWrites)
        {
            throw new PictureStorageException("Simulated storage failure.");
        }

        Objects[objectName] = jpeg;
        return Task.CompletedTask;
    }

    public Task<byte[]?> GetAsync(string objectName, CancellationToken cancellationToken) =>
        Task.FromResult(Objects.TryGetValue(objectName, out var bytes) ? bytes : null);

    public Task DeleteAsync(string objectName, CancellationToken cancellationToken)
    {
        Objects.TryRemove(objectName, out _);
        return Task.CompletedTask;
    }
}
