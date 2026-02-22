English | [繁體中文](README.zh-TW.md)

# CoreMesh.Dispatching

`CoreMesh.Dispatching` is a lightweight dispatching module for CoreMesh that provides:

- `Send` for request/response
- `Send` for commands without a response payload
- `Publish` for notifications/events

Design goals:

- Simple
- Readable
- Easy to learn
- Low runtime overhead (lazy cache)

## Current Features

- `Dispatcher` uses wrapper + cache (lazy loading)
- Notifications are executed sequentially by default (safety-first)
- Supports `Microsoft.Extensions.DependencyInjection` registration and assembly scanning
- No pipeline (intentionally excluded)

## Core Interfaces

- `IRequest<TResponse>`: request with response
- `IRequest`: command without response payload
- `IRequestHandler<TRequest, TResponse>`
- `IRequestHandler<TRequest>`
- `INotification`: event/notification
- `INotificationHandler<TNotification>`
- `IDispatcher`

## Quick Start

### 1. Define a request/response and handler

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

### 2. Register Dispatcher and handlers

```csharp
using CoreMesh.Dispatching;

builder.Services.AddDispatching(typeof(SampleHandler).Assembly);
```

Or register manually:

```csharp
builder.Services.AddScoped<IDispatcher, Dispatcher>();
builder.Services.AddScoped<IRequestHandler<SampleQuery, SampleResponse>, SampleHandler>();
```

### 3. Call `Send`

```csharp
app.MapGet("/sample", async (IDispatcher dispatcher, CancellationToken ct) =>
{
    var result = await dispatcher.Send(new SampleQuery("Foo", "Bar"), ct);
    return Results.Ok(result);
});
```

## Notification Example (`Publish`)

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

Registration:

```csharp
builder.Services.AddScoped<INotificationHandler<UserCreated>, AuditLogOnUserCreatedHandler>();
builder.Services.AddScoped<INotificationHandler<UserCreated>, WelcomeEmailOnUserCreatedHandler>();
```

Invocation:

```csharp
await dispatcher.Publish(new UserCreated(123, "demo@coremesh.dev"), ct);
```

## Behavior Notes

### Send

- `Send(IRequest<TResponse>)`: requires exactly one matching handler
- `Send(IRequest)`: requires exactly one matching handler
- If no handler is registered, DI resolution throws (`InvalidOperationException`)

### Publish

- `Publish(INotification)` dispatches to all matching `INotificationHandler<T>`
- Execution is **sequential** (in registration order)
- This is safer when handlers share scoped dependencies

## Use Cases (When This Pattern Fits)

`CoreMesh.Dispatching` follows an application-layer request dispatching (Mediator-like) pattern. It is suitable for:

### 1. Application orchestration in Web API / Minimal API

When endpoints should stay thin and delegate use-case execution to handlers instead of calling services/repositories directly.

Good for:
- Queries
- Commands
- Application workflow orchestration

Benefits:
- Thinner endpoints
- Use-case logic is centralized in handlers
- Easier testing

### 2. Splitting large application services (avoiding a God Service)

Replace a large `ApplicationService` with many focused request handlers, each handling one use case.

Good for:
- Growing codebases
- Team collaboration (ownership by feature/handler)

### 3. Notification-driven follow-up actions

After a primary workflow completes, use `Publish` to trigger multiple follow-up side effects.

Examples:
- After user creation: audit log, welcome email, metrics
- After payment: reporting update, external notification, outbox write

### 4. A lightweight unified entry point without a full framework

When you do not want a full CQRS/DDD framework yet, but still want a small, predictable dispatching abstraction.

## What This Module Handles

`CoreMesh.Dispatching` is responsible for request/notification dispatching and invocation, not business logic itself.

### Included Responsibilities

- `Request -> Handler` dispatch (`Send`)
- `Notification -> Handlers` dispatch (`Publish`)
- Handler resolution from DI
- Handler discovery and registration (assembly scanning)
- Runtime wrapper cache (lazy cache)
- Wrapper factory cache for first-time type wrapping

### Intentionally Not Included (Current Version)

- Validation pipeline
- Logging pipeline
- Retry / Circuit breaker
- Transaction / Unit of Work
- Authorization
- ASP.NET Core endpoint abstraction
- Outbox / message broker delivery

These are cross-cutting concerns or infrastructure integrations and are better handled in outer modules (endpoint layer, middleware, decorators, background jobs).

## Design Trade-offs

### Why no Pipeline?

This version intentionally excludes pipeline support to keep:

- Lower latency
- Fewer allocations
- A simpler execution path

Cross-cutting concerns such as validation, logging, or proxy behavior can be handled outside the dispatcher (for example: endpoint layer, middleware, decorators).

## Notes

- Prefer explicit assembly scanning (for example `typeof(SomeHandler).Assembly`)
- `AddDispatching()` without parameters scans currently loaded assemblies (higher startup cost)
- A request type should correspond to a single response type (`IRequest<TResponse>`)

## Future Directions

- `AddDispatchingFromAssemblyContaining<T>()`
- More DI registration options (lifetime / filters)
- Configurable notification publisher strategies (sequential / parallel)
- Source generator support to further reduce runtime type-resolution overhead
