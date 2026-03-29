[English](README.md) | 繁體中文

# CoreMesh.Middleware

可組合的 ASP.NET Core Middleware 函式庫。只需註冊需要的功能，未使用的 Middleware 不會被加入管線。

## 安裝

```bash
dotnet add package CoreMesh.Middleware
```

## 設定

```csharp
// Program.cs

// 1. 註冊服務
builder.Services.AddCoreMeshMiddleware(middleware =>
{
    middleware.AddIdempotency(idempotency =>
    {
        idempotency.WithHandler<RedisIdempotencyHandler>();
        idempotency.Configure(opt =>
        {
            opt.CacheExpiry = TimeSpan.FromHours(24);
        });
    });
});

// 2. 加入管線
app.UseCoreMeshMiddleware();
```

`UseCoreMeshMiddleware()` 只會啟用在 `AddCoreMeshMiddleware()` 中註冊的 Middleware，加入管線的順序依照註冊順序。

## 可用 Middleware

| Middleware | 說明 |
|---|---|
| [Idempotency](./Idempotency/README.zh-TW.md) | 防止相同的 POST 請求被重複執行 |