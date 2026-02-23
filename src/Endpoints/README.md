[繁體中文](README.zh-TW.md) | English

# CoreMesh.Endpoints

`CoreMesh.Endpoints` provides a lightweight endpoint discovery and mapping abstraction for ASP.NET Core.

It allows you to:

1. Define endpoints as classes
2. Register endpoint implementations through assembly scanning
3. Map normal endpoints and grouped endpoints in one place

## Features

- `IEndpoint`: map root endpoints
- `IGroupEndpoint`: define a route group and configure shared metadata
- `IGroupedEndpoint<TGroup>`: map endpoints under a specific group
- `AddEndpoints()`: scan loaded assemblies and register endpoint types
- `MapEndpoints()`: map registered endpoints to `WebApplication`

## Installation

Reference the project/package and call:

```csharp
using CoreMesh.Endpoints.Extensions;

builder.Services.AddEndpoints();

var app = builder.Build();

app.MapEndpoints();
```

## Core Interfaces

### `IEndpoint`

Use for root endpoints:

```csharp
using CoreMesh.Endpoints;

public sealed class PingEndpoint : IEndpoint
{
    public void AddRoute(IEndpointRouteBuilder app)
    {
        app.MapGet("/ping", () => Results.Ok("pong"));
    }
}
```

### `IGroupEndpoint` + `IGroupedEndpoint<TGroup>`

Use when you want a route group with shared configuration:

```csharp
using CoreMesh.Endpoints;

public sealed class ProductsGroup : IGroupEndpoint
{
    public string GroupPrefix => "/products";

    public void Configure(RouteGroupBuilder group)
    {
        group.WithTags("Products");
    }
}

public sealed class GetProductsEndpoint : IGroupedEndpoint<ProductsGroup>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapGet("/", () => Results.Ok(Array.Empty<object>()));
    }
}
```

## Design Notes (Current Version)

- Endpoint discovery uses `AppDomain.CurrentDomain.GetAssemblies()`
- Endpoints are currently registered as `Singleton`
- Mapping is convention-based using implemented interfaces (`IEndpoint`, `IGroupEndpoint`, `IGroupedEndpoint`)

These behaviors are intentionally kept close to the original implementation and may be refined in later iterations.

## Testing

Current tests cover:

1. `AddEndpoints()` registration for endpoint/group/grouped endpoint types
2. `MapEndpoints()` invocation of `AddRoute()` and group mapping behavior

