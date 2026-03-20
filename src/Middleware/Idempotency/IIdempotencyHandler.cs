namespace CoreMesh.Middleware.Idempotency;

public interface IIdempotencyHandler
{
    Task<IdempotencyResult?> GetExistingResponseAsync(string idempotencyKey, CancellationToken cancellationToken = default);
    Task StoreResponseAsync(string idempotencyKey, int statusCode, string responsePayload, CancellationToken cancellationToken = default);
}

public record IdempotencyResult(int StatusCode, string Payload);

public class DefaultIdempotencyHandler : IIdempotencyHandler
{
    public Task<IdempotencyResult?> GetExistingResponseAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IdempotencyResult?>(null);
    }

    public Task StoreResponseAsync(string idempotencyKey, int statusCode, string responsePayload,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
