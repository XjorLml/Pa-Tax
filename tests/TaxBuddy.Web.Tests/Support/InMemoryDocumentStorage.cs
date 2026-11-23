using System.Collections.Concurrent;
using TaxBuddy.Web.Application.Work;

namespace TaxBuddy.Web.Tests.Support;

/// <summary>Stands in for the service documents bucket in web tests (F03 stage 2). Can be told to fail writes, reads or deletes.</summary>
public sealed class InMemoryDocumentStorage : IDocumentStorage
{
    public ConcurrentDictionary<string, (byte[] Content, string ContentType)> Objects { get; } = new();

    public bool FailWrites { get; set; }

    public bool FailDeletes { get; set; }

    public bool IsAvailable => true;

    public Task PutAsync(string objectName, byte[] content, string contentType, CancellationToken cancellationToken)
    {
        if (FailWrites)
        {
            throw new DocumentStorageException("Simulated storage failure.");
        }

        Objects[objectName] = (content, contentType);
        return Task.CompletedTask;
    }

    public Task<byte[]?> GetAsync(string objectName, CancellationToken cancellationToken) =>
        Task.FromResult(Objects.TryGetValue(objectName, out var stored) ? stored.Content : null);

    public Task DeleteAsync(string objectName, CancellationToken cancellationToken)
    {
        if (FailDeletes)
        {
            throw new DocumentStorageException("Simulated storage failure.");
        }

        Objects.TryRemove(objectName, out _);
        return Task.CompletedTask;
    }
}
