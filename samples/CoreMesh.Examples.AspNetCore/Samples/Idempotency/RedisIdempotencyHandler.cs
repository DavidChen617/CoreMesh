using System.Text.Json;
using CoreMesh.Middleware.Idempotency;
using StackExchange.Redis;

namespace CoreMesh.Examples.AspNetCore.Samples.Idempotency;

public class RedisIdempotencyHandler(IConnectionMultiplexer redis, IdempotencyOptions options)
    : IIdempotencyHandler
{
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task<IdempotencyResult?> GetExistingResponseAsync(
        string idempotencyKey, CancellationToken cancellationToken = default)
    {
        var value = await _db.StringGetAsync(idempotencyKey);
        if (value.IsNullOrEmpty)
            return null;

        var cached = JsonSerializer.Deserialize<CachedResponse>((string)value!);
        return new IdempotencyResult(cached!.StatusCode, cached.Payload);
    }

    public async Task StoreResponseAsync(
        string idempotencyKey, int statusCode, string responsePayload,
        CancellationToken cancellationToken = default)
    {
        var cached = new CachedResponse(statusCode, responsePayload);
        await _db.StringSetAsync(
            idempotencyKey,
            JsonSerializer.Serialize(cached),
            expiry: options.CacheExpiry);
    }

    private record CachedResponse(int StatusCode, string Payload);
}
