[English](README.md) | 繁體中文

# Idempotency Middleware

防止相同的 POST 請求被重複執行。當 Client 使用相同的 Idempotency Key 重試請求時，Middleware 會直接回傳原本快取的 Response，不會再次執行業務邏輯。

## 運作流程

```
收到 POST 請求
  └─ 有 [Idempotency] Attribute？     → 否 → 直接放行
       └─ 有帶 Idempotency Key Header？ → 否 → 400 Bad Request
            └─ 快取中找到此 Key？       → 是 → 回傳快取 Response（X-Idempotency-Replayed: true）
                 └─ 否 → 執行 Handler，2xx 時儲存 Response
```

## 基本用法

在 Endpoint 加上 `[Idempotency]` 標記：

```csharp
// Minimal API
app.MapPost("orders", [Idempotency] async (CreateOrderRequest request, ...) =>
{
    // 每個唯一的 Idempotency-Key 只會執行一次
});

// Controller
[Idempotency]
[HttpPost("orders")]
public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request) { ... }
```

Client 送出：
```http
POST /orders
Idempotency-Key: 550e8400-e29b-41d4-a716-446655440000
Content-Type: application/json
```

相同 Key 的第二次請求會回傳原始 Response，並附加額外 Header：
```http
HTTP/1.1 200 OK
X-Idempotency-Replayed: true
```

## 自訂 Header 名稱

```csharp
app.MapPost("payments", [Idempotency("X-Payment-Key")] (...) => { ... });
```

```http
POST /payments
X-Payment-Key: pay-550e8400
```

## 設定選項

```csharp
middleware.AddIdempotency(idempotency =>
{
    idempotency.Configure(opt =>
    {
        opt.IdempotencyKeyName        = "Idempotency-Key";          // 預設 Header 名稱
        opt.IdempotencyHeaderReplayed = "X-Idempotency-Replayed";   // 重播標記 Header
        opt.CacheExpiry               = TimeSpan.FromHours(24);     // Response 快取時間

        opt.ErrorResponseFormatter = (message, httpContext) =>
            JsonSerializer.Serialize(new { error = message, path = httpContext.Request.Path });
    });
});
```

## 實作自訂 Handler

預設的 `DefaultIdempotencyHandler` 不做任何儲存，生產環境必須自行實作。

### 介面

```csharp
public interface IIdempotencyHandler
{
    Task<IdempotencyResult?> GetExistingResponseAsync(string idempotencyKey, CancellationToken cancellationToken = default);
    Task StoreResponseAsync(string idempotencyKey, int statusCode, string responsePayload, CancellationToken cancellationToken = default);
}
```

### Redis（建議用於生產環境）

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

註冊：

```csharp
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(configuration["Redis:ConnectionString"]!));

builder.Services.AddCoreMeshMiddleware(middleware =>
    middleware.AddIdempotency(idempotency =>
        idempotency.WithHandler<RedisIdempotencyHandler>()
    )
);
```

### Handler 生命週期建議

| Handler 類型 | 建議 | 原因 |
|---|---|---|
| 記憶體 | `WithHandler(new MyHandler())` → Singleton | 需要跨 Request 共享狀態 |
| Redis | `WithHandler<RedisHandler>()` → Scoped 或 Singleton | `IConnectionMultiplexer` 是 Singleton |
| EF Core / DbContext | `WithHandler<DbHandler>()` → Scoped | `DbContext` 必須是 Scoped |

## 生產環境注意事項

**此 Middleware 是第一道防線，不是唯一的保護。**

對於關鍵操作（金流、建立訂單），請搭配資料庫層級的唯一約束：

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
    // 兩個並發請求同時通過 Middleware，返回已存在的資料
    return await dbContext.Orders.FirstAsync(o => o.IdempotencyKey == key);
}
```

Middleware 負責快速路徑（快取命中），DB 約束保證並發情況下的資料正確性。