namespace CoreMesh.Middleware.Idempotency;

/// <summary>
/// Defines storage operations for idempotency key/response pairs.
/// Implement this interface to plug in a custom backend (e.g. Redis, SQL).
/// </summary>
public interface IIdempotencyHandler
{
    /// <summary>
    /// Returns a previously stored response for the given idempotency key,
    /// or <see langword="null"/> if no entry exists.
    /// </summary>
    /// <param name="idempotencyKey">The unique key sent by the client.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IdempotencyResult?> GetExistingResponseAsync(string idempotencyKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists the response associated with the given idempotency key so that
    /// subsequent duplicate requests can be replayed without re-executing the handler.
    /// </summary>
    /// <param name="idempotencyKey">The unique key sent by the client.</param>
    /// <param name="statusCode">The HTTP status code of the original response.</param>
    /// <param name="responsePayload">The serialized response body.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task StoreResponseAsync(string idempotencyKey, int statusCode, string responsePayload, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a cached HTTP response for idempotency replay.
/// </summary>
/// <param name="StatusCode">The original HTTP status code.</param>
/// <param name="Payload">The serialized response body.</param>
public record IdempotencyResult(int StatusCode, string Payload);

/// <summary>
/// No-op default implementation of <see cref="IIdempotencyHandler"/>.
/// Does not persist or retrieve any responses — every request is treated as new.
/// Replace with a real backend (Redis, database) for production use.
/// </summary>
public class DefaultIdempotencyHandler : IIdempotencyHandler
{
    /// <inheritdoc/>
    public Task<IdempotencyResult?> GetExistingResponseAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IdempotencyResult?>(null);
    }

    /// <inheritdoc/>
    public Task StoreResponseAsync(string idempotencyKey, int statusCode, string responsePayload,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
