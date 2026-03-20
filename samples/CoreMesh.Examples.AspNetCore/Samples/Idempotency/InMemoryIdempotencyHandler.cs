using System.Collections.Concurrent;
using CoreMesh.Middleware.Idempotency;

namespace CoreMesh.Examples.AspNetCore.Samples.Idempotency;

public class InMemoryIdempotencyHandler : IIdempotencyHandler
{
    private readonly ConcurrentDictionary<string, IdempotencyResult> _store = new();

    public Task<IdempotencyResult?> GetExistingResponseAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(idempotencyKey, out var result);
        return Task.FromResult(result);
    }

    public Task StoreResponseAsync(string idempotencyKey, int statusCode, string responsePayload, CancellationToken cancellationToken = default)
    {
        _store[idempotencyKey] = new IdempotencyResult(statusCode, responsePayload);
        return Task.CompletedTask;
    }
}
