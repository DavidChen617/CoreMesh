[English](README.md) | 繁體中文

# CoreMesh.Dispatching.Abstractions

CoreMesh dispatching 系統的純 contract 套件，不含任何執行期實作。適合只想依賴介面、不依賴具體實作的場景。

一般應用程式專案建議直接使用 `CoreMesh.Dispatching`（已包含這些 abstractions）。

## 介面

| 介面 | 說明 |
|------|------|
| `IRequest<TResponse>` | 有回應的 request |
| `IRequest` | 無回應的 command |
| `IRequestHandler<TRequest, TResponse>` | 處理有回應的 request |
| `IRequestHandler<TRequest>` | 處理無回應的 command |
| `INotification` | 事件 / 通知的 marker interface |
| `INotificationHandler<TNotification>` | 處理通知 |
| `ISender` | 發送 request 的入口點 |
| `IPublisher` | 發布 notification 的入口點 |
| `IDispatcher` | 結合 `ISender` 與 `IPublisher` |
| `INotificationPublisher` | 定義 notification 分派策略 |

## 型別

| 型別 | 說明 |
|------|------|
| `NotificationHandlerExecutor` | 包裝 handler 實例與型別化呼叫 callback |
| `Unit` | 代替 void 的回傳型別；`Unit.Task` 為共享的快取 `Task<Unit>` |

## 範例

```csharp
public sealed record GetUserQuery(int UserId) : IRequest<UserDto>;

public sealed class GetUserHandler : IRequestHandler<GetUserQuery, UserDto>
{
    public Task<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken = default)
        => Task.FromResult(new UserDto(request.UserId, "Alice"));
}
```
