English | [繁體中文](README.zh-TW.md)

# CoreMesh.Middleware

A composable ASP.NET Core middleware library for CoreMesh. Register only what you need — unused middleware is never added to the pipeline.

## Installation

```bash
dotnet add package CoreMesh.Middleware
```

## Setup

```csharp
// Program.cs

// 1. Register services
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

// 2. Add to pipeline
app.UseCoreMeshMiddleware();
```

`UseCoreMeshMiddleware()` automatically uses only the middleware registered during `AddCoreMeshMiddleware()`. Order in the pipeline follows registration order.

## Available Middleware

| Middleware | Description |
|---|---|
| [Idempotency](./Idempotency/README.md) | Prevents duplicate POST requests from being processed more than once |