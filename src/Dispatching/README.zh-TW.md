[English](README.md) | 繁體中文

# CoreMesh.Dispatching

`CoreMesh.Dispatching` 是 CoreMesh 的輕量 Dispatching 模組，提供：

- `Send`：請求/回應（request/response）
- `Send`：無回傳 command
- `Publish`：事件通知（notification）

設計目標：

- 簡單
- 可讀
- 易學習
- 低執行成本（lazy cache）

## 目前特性

- `Dispatcher` 使用 wrapper + cache（懶加載）
- Notification 預設為串行執行（安全優先）
- 可配置 notification publisher 策略（串行 / 並行）
- 支援 `Microsoft.Extensions.DependencyInjection` 註冊與 assembly 掃描
- 不包含 pipeline

## 核心介面

- `IRequest<TResponse>`：有回傳的請求
- `IRequest`：無回傳 command
- `IRequestHandler<TRequest, TResponse>`
- `IRequestHandler<TRequest>`
- `INotification`：事件通知
- `INotificationHandler<TNotification>`
- `ISender`：request 發送入口
- `IPublisher`：notification 發布入口
- `IDispatcher`：結合 `ISender` 和 `IPublisher`

## 快速開始

### 1. 定義 request/response 與 handler

```csharp
using CoreMesh.Dispatching;

public sealed record SampleQuery(string Foo, string Bar) : IRequest<SampleResponse>;

public sealed record SampleResponse(string Foo, string Bar);

public sealed class SampleHandler : IRequestHandler<SampleQuery, SampleResponse>
{
    public Task<SampleResponse> Handle(SampleQuery request, CancellationToken cancellationToken = default)
        => Task.FromResult(new SampleResponse(request.Foo, request.Bar));
}
```

### 2. 註冊 Dispatcher 與 Handlers

```csharp
using CoreMesh.Dispatching.Extensions;

builder.Services.AddDispatching(typeof(SampleHandler).Assembly);
```

或手動註冊：

```csharp
using CoreMesh.Dispatching.Notification;
using CoreMesh.Dispatching.Notification.Publisher;

builder.Services.AddSingleton<INotificationPublisher, ForeachAwaitPublisher>();
builder.Services.AddScoped<IDispatcher, Dispatcher>();
builder.Services.AddScoped<IRequestHandler<SampleQuery, SampleResponse>, SampleHandler>();
```

### 3. 呼叫 `Send`

```csharp
app.MapGet("/sample", async (IDispatcher dispatcher, CancellationToken ct) =>
{
    var result = await dispatcher.Send(new SampleQuery("Foo", "Bar"), ct);
    return Results.Ok(result);
});
```

## Notification 範例（Publish）

```csharp
using CoreMesh.Dispatching;

public sealed record UserCreated(int UserId, string Email) : INotification;

public sealed class AuditLogOnUserCreatedHandler : INotificationHandler<UserCreated>
{
    public Task Handle(UserCreated notification, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[Audit] User created: {notification.UserId}");
        return Task.CompletedTask;
    }
}

public sealed class WelcomeEmailOnUserCreatedHandler : INotificationHandler<UserCreated>
{
    public Task Handle(UserCreated notification, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[Mail] Send welcome email to {notification.Email}");
        return Task.CompletedTask;
    }
}
```

註冊：

```csharp
builder.Services.AddScoped<INotificationHandler<UserCreated>, AuditLogOnUserCreatedHandler>();
builder.Services.AddScoped<INotificationHandler<UserCreated>, WelcomeEmailOnUserCreatedHandler>();
```

呼叫：

```csharp
await dispatcher.Publish(new UserCreated(123, "demo@coremesh.dev"), ct);
```

## 行為說明

### Send

- `Send(IRequest<TResponse>)`：必須有且只有一個對應 handler
- `Send(IRequest)`：必須有且只有一個對應 handler
- 若找不到 handler，會拋出 DI 解析例外（`InvalidOperationException`）

### Publish

- `Publish(INotification)` 會執行所有對應的 `INotificationHandler<T>`
- 預設為 **串行執行**（依註冊順序）
- 適合安全優先場景（例如 handler 共用 scoped 依賴）
- 可配置為 **並行執行** 以提高吞吐量：

```csharp
builder.Services.AddDispatching(
    options => options.UseParallelPublisher(),
    typeof(Program).Assembly);
```

## 適用場景（這個設計模式適合用在哪裡）

`CoreMesh.Dispatching` 採用的是「應用層請求分派（Dispatcher / Mediator-like）」模式，適合以下場景：

### 1. Web API / Minimal API 的應用層協調

當 endpoint 不想直接依賴 service/repository，改為送出一個 request 給對應 handler 處理。

適合：
- 查詢（Query）
- 命令（Command）
- 應用層流程協調

效果：
- endpoint 更薄
- 業務流程集中在 handler
- 邏輯更容易測試

### 2. 單一職責拆分（避免 God Service）

把大量方法的 `ApplicationService` 拆成多個 request handler，每個 handler 只處理一個 use case。

適合：
- 功能逐步增長的專案
- 團隊多人協作（每人維護不同 handler）

### 3. 事件通知（Notification）驅動的後續動作

一個主流程完成後，透過 `Publish` 廣播事件，讓多個 handler 處理後續副作用。

例如：
- 使用者建立後：寫審計、寄信、記錄 metrics
- 訂單付款後：更新報表、通知外部系統、寫 outbox

### 4. 想保留簡單架構，但需要統一入口

不想一開始導入完整 CQRS/DDD 框架，只需要一個輕量、可控、低開銷的分派機制。

## 這個模組幫你處理了哪些事情

`CoreMesh.Dispatching` 主要處理的是「請求/通知的分派與調用」，而不是業務邏輯本身。

### 已處理的事情

- `Request -> Handler` 對應與呼叫（`Send`）
- `Notification -> 多個 Handlers` 的分派（`Publish`）
- DI 容器中的 handler 解析
- Handler 探索與註冊（assembly 掃描）
- 執行期 wrapper 快取（lazy cache）
- 首次型別包裝建立（wrapper factory cache）

### 刻意不處理的事情（目前版本）

- Validation pipeline
- Logging pipeline
- Retry / Circuit breaker
- Transaction / Unit of Work
- Authorization
- ASP.NET Core endpoint 抽象
- Outbox / Message broker 發送

這些屬於 cross-cutting concern 或基礎設施整合，建議放在外層模組處理（例如 endpoint、middleware、decorator、背景工作）。

## 設計取捨

### 為什麼沒有 Pipeline？

目前版本刻意不包含 pipeline，以保持：

- 更低延遲
- 更少配置
- 更簡單的執行路徑

Validation / logging / proxy 等 cross-cutting concern 建議先放在外層（例如 endpoint、middleware、decorator）。

## 注意事項

- `AddDispatching()` 需要至少傳入一個 assembly 參數
- 建議明確傳入 assembly 掃描（例如 `typeof(SomeHandler).Assembly`）
- 一個 request 型別應對應一種回應型別（`IRequest<TResponse>`）

## 效能

與 MediatR 12.x的 Benchmark 比較：

| 方法                       | 平均時間   | 記憶體分配 |
|--------------------------- |----------:|----------:|
| Baseline（直接呼叫）        |  10.97 ns |     104 B |
| CoreMesh Send              |  27.55 ns |     104 B |
| MediatR Send               |  50.20 ns |     232 B |
| CoreMesh Publish (1)       |  59.29 ns |     232 B |
| MediatR Publish (1)        |  68.73 ns |     288 B |
| CoreMesh Publish (5)       | 143.98 ns |     712 B |
| MediatR Publish (5)        | 166.18 ns |     896 B |

CoreMesh 在 Send 上快約 45%，Publish 快約 15-20%，記憶體分配少約 20-55%。
