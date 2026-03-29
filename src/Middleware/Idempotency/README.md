English | [繁體中文](README.zh-TW.md)

# Idempotency Middleware

Prevents duplicate POST requests from being executed more than once. When a client retries a request with the same idempotency key, the middleware replays the original cached response without re-executing the handler.

## How It Works

```
Incoming POST request
  └─ Has [Idempotency] attribute?       → No  → pass through
       └─ Has idempotency key header?   → No  → 400 Bad Request
            └─ Key found in cache?      → Yes → replay cached response (X-Idempotency-Replayed: true)
                 └─ No → execute handler, store response on 2xx
```

## Basic Usage

Mark any endpoint with `[Idempotency]`:

```csharp
// Minimal API
app.MapPost("orders", [Idempotency] async (CreateOrderRequest request, ...) =>
{
    // executed only once per unique Idempotency-Key
});

// Controller
[Idempotency]
[HttpPost("orders")]
public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request) { ... }
```

Client sends:
```http
POST /orders
Idempotency-Key: 550e8400-e29b-41d4-a716-446655440000
Content-Type: application/json
```

Second request with the same key returns the original response with an extra header:
```http
HTTP/1.1 200 OK
X-Idempotency-Replayed: true
```

## Custom Header Name

```csharp
app.MapPost("payments", [Idempotency("X-Payment-Key")] (...) => { ... });
```

```http
POST /payments
X-Payment-Key: pay-550e8400
```

## Configuration

```csharp
middleware.AddIdempotency(idempotency =>
{
    idempotency.Configure(opt =>
    {
        opt.IdempotencyKeyName        = "Idempotency-Key";          // default header name
        opt.IdempotencyHeaderReplayed = "X-Idempotency-Replayed";   // replay indicator header
        opt.CacheExpiry               = TimeSpan.FromHours(24);     // how long to cache responses

        opt.ErrorResponseFormatter = (message, httpContext) =>
            JsonSerializer.Serialize(new { error = message, path = httpContext.Request.Path });
    });
});
```

## Implementing a Custom Handler

The default `DefaultIdempotencyHandler` does nothing (no storage). Provide your own implementation for production use.

### Interface

```csharp
public interface IIdempotencyHandler
{
    Task<IdempotencyResult?> GetExistingResponseAsync(string idempotencyKey, CancellationToken cancellationToken = default);
    Task StoreResponseAsync(string idempotencyKey, int statusCode, string responsePayload, CancellationToken cancellationToken = default);
}
```

### Redis (recommended for production)

```csharp
public class RedisIdempotencyHandler(IConnectionMultiplexer redis, IdempotencyOptions options)
    : IIdempotencyHandler
{
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task<IdempotencyResult?> GetExistingResponseAsync(
        string idempotencyKey, CancellationToken cancellationToken = default)
    {
        var value = await _db.StringGetAsync(idempotencyKey);
        if (value.IsNullOrEmpty) return null;

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
```

Register:

```csharp
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(configuration["Redis:ConnectionString"]!));

builder.Services.AddCoreMeshMiddleware(middleware =>
    middleware.AddIdempotency(idempotency =>
        idempotency.WithHandler<RedisIdempotencyHandler>()
    )
);
```

### Handler Registration Lifetimes

| Handler type | Recommended | Reason |
|---|---|---|
| In-memory | `WithHandler(new MyHandler())` → Singleton | Must share state across requests |
| Redis | `WithHandler<RedisHandler>()` → Scoped or Singleton | `IConnectionMultiplexer` is Singleton |
| EF Core / DbContext | `WithHandler<DbHandler>()` → Scoped | `DbContext` must be Scoped |

## Production Considerations

**This middleware is the first line of defense, not the only one.**

For critical operations (payments, order creation), combine with a database-level unique constraint:

```sql
ALTER TABLE orders ADD idempotency_key VARCHAR(100) UNIQUE;
```

```csharp
try
{
    await dbContext.Orders.AddAsync(order);
    await dbContext.SaveChangesAsync();
}
catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
{
    // two concurrent requests slipped through — return the existing record
    return await dbContext.Orders.FirstAsync(o => o.IdempotencyKey == key);
}
```

The middleware handles the fast path (cache hit). The DB constraint guarantees correctness under concurrent load.