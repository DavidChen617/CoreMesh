English | [繁體中文](README.zh-TW.md)

# CoreMesh.Dispatching.Abstractions

Contracts-only package for the CoreMesh dispatching system. No runtime implementation — reference this when you want to depend on interfaces only.

For most application projects, use `CoreMesh.Dispatching` instead (it already includes these abstractions).

## Interfaces

| Interface | Description |
|-----------|-------------|
| `IRequest<TResponse>` | Request that returns a response |
| `IRequest` | Command without a response payload |
| `IRequestHandler<TRequest, TResponse>` | Handles a request with response |
| `IRequestHandler<TRequest>` | Handles a command without response |
| `INotification` | Marker interface for events/notifications |
| `INotificationHandler<TNotification>` | Handles a notification |
| `ISender` | Sends requests |
| `IPublisher` | Publishes notifications |
| `IDispatcher` | Combines `ISender` and `IPublisher` |
| `INotificationPublisher` | Defines notification dispatch strategy |

## Types

| Type | Description |
|------|-------------|
| `NotificationHandlerExecutor` | Wraps a handler instance and its typed invocation callback |
| `Unit` | Void return type substitute; `Unit.Task` is a shared cached `Task<Unit>` |

## Example

```csharp
public sealed record GetUserQuery(int UserId) : IRequest<UserDto>;

public sealed class GetUserHandler : IRequestHandler<GetUserQuery, UserDto>
{
    public Task<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken = default)
        => Task.FromResult(new UserDto(request.UserId, "Alice"));
}
```
