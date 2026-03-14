# CoreMesh

A collection of lightweight, modular .NET libraries designed to keep your application layer clean without pulling in heavy dependencies.

Each package is independent — use only what you need.

---

## Packages

| Package | Description | NuGet |
|---------|-------------|-------|
| [CoreMesh.Dispatching](src/Dispatching/README.md) | Lightweight mediator-style request/notification dispatcher | [![NuGet](https://img.shields.io/nuget/v/CoreMesh.Dispatching)](https://www.nuget.org/packages/CoreMesh.Dispatching) |
| [CoreMesh.Dispatching.Abstractions](src/Dispatching.Abstractions/README.md) | Contracts only — interfaces for requests, handlers, notifications | [![NuGet](https://img.shields.io/nuget/v/CoreMesh.Dispatching.Abstractions)](https://www.nuget.org/packages/CoreMesh.Dispatching.Abstractions) |
| [CoreMesh.Validation](src/Validation/README.md) | Fluent property validation with rule caching | [![NuGet](https://img.shields.io/nuget/v/CoreMesh.Validation)](https://www.nuget.org/packages/CoreMesh.Validation) |
| [CoreMesh.Validation.Abstractions](src/Validation.Abstractions/README.md) | Contracts and built-in rule extensions for validation | [![NuGet](https://img.shields.io/nuget/v/CoreMesh.Validation.Abstractions)](https://www.nuget.org/packages/CoreMesh.Validation.Abstractions) |
| [CoreMesh.Result](src/Result/README.md) | Strongly-typed result pattern with status codes and error details | [![NuGet](https://img.shields.io/nuget/v/CoreMesh.Result)](https://www.nuget.org/packages/CoreMesh.Result) |
| [CoreMesh.Result.AspNetCore](src/Result.AspNetCore/README.md) | ASP.NET Core integration — HTTP response conversion and global exception handling | [![NuGet](https://img.shields.io/nuget/v/CoreMesh.Result.AspNetCore)](https://www.nuget.org/packages/CoreMesh.Result.AspNetCore) |
| [CoreMesh.Mapper](src/Mapper/README.md) | Convention-based object mapper with strongly-typed contracts | [![NuGet](https://img.shields.io/nuget/v/CoreMesh.Mapper)](https://www.nuget.org/packages/CoreMesh.Mapper) |
| [CoreMesh.Mapper.Extensions](src/Mapper.Extensions/README.md) | DI registration for CoreMesh.Mapper | [![NuGet](https://img.shields.io/nuget/v/CoreMesh.Mapper.Extensions)](https://www.nuget.org/packages/CoreMesh.Mapper.Extensions) |
| [CoreMesh.Interception](src/Interception/README.md) | AOP interception via DispatchProxy with sync and async support | [![NuGet](https://img.shields.io/nuget/v/CoreMesh.Interception)](https://www.nuget.org/packages/CoreMesh.Interception) |
| [CoreMesh.Interception.Extensions](src/Interception.Extensions/README.md) | DI registration for CoreMesh.Interception | [![NuGet](https://img.shields.io/nuget/v/CoreMesh.Interception.Extensions)](https://www.nuget.org/packages/CoreMesh.Interception.Extensions) |
| [CoreMesh.Endpoints](src/Endpoints/README.md) | Minimal API endpoint pattern for organized route registration | [![NuGet](https://img.shields.io/nuget/v/CoreMesh.Endpoints)](https://www.nuget.org/packages/CoreMesh.Endpoints) |
| [CoreMesh.Logging](src/Logging/README.md) | File logger provider with rotation and category filtering | [![NuGet](https://img.shields.io/nuget/v/CoreMesh.Logging)](https://www.nuget.org/packages/CoreMesh.Logging) |

---

## Design Goals

- **Modular** — each package solves one problem. No forced bundling.
- **Minimal dependencies** — most packages depend only on `Microsoft.Extensions.*` abstractions.
- **Readable** — small surface area, straightforward APIs, no hidden magic.
- **Performant** — lazy caching and delegate compilation keep runtime overhead low.

---

## Quick Start

A typical ASP.NET Core setup combining the most common packages:

```csharp
// Program.cs
builder.Services.AddDispatching(typeof(Program).Assembly);
builder.Services.AddValidatable(typeof(Program).Assembly);
builder.Services.AddCoreMeshMapper(typeof(Program).Assembly);
builder.Services.AddCoreMeshExceptionHandling();

app.UseCoreMeshExceptionHandling();
```

```csharp
// Define a request
public sealed record CreateUserCommand(string? Name, string? Email)
    : IRequest<Result<UserDto>>, IValidatable<CreateUserCommand>
{
    public void ConfigureValidateRules(IValidationBuilder<CreateUserCommand> builder)
    {
        builder.For(x => x.Name).NotNull().NotEmpty().MaxLength(100);
        builder.For(x => x.Email).NotNull().EmailAddress();
    }
}

// Handle it
public sealed class CreateUserHandler(IValidator validator) : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    public Task<Result<UserDto>> Handle(CreateUserCommand command, CancellationToken ct = default)
    {
        var validation = validator.Validate(command);
        if (!validation.IsValid)
            return Task.FromResult(Result<UserDto>.Invalid(validation.Errors));

        // ... business logic ...
        return Task.FromResult(Result<UserDto>.Ok(new UserDto(...)));
    }
}

// Expose via endpoint
app.MapPost("/users", async (CreateUserCommand cmd, IDispatcher dispatcher, CancellationToken ct) =>
    (await dispatcher.Send(cmd, ct)).ToHttpResult());
```

---

## Performance

Benchmarks run on .NET 10, Apple M2 Pro.

### Dispatching

| Method | Mean | Allocated |
|--------|-----:|----------:|
| Direct call (baseline) | 10.97 ns | 104 B |
| CoreMesh Send | 27.55 ns | 104 B |
| CoreMesh Publish (1 handler) | 59.29 ns | 232 B |
| CoreMesh Publish (5 handlers) | 143.98 ns | 712 B |

### Validation

| Scenario | CoreMesh | Popular alternative | Speedup |
|----------|----------|---------------------|---------|
| Single valid | 206 ns | 347 ns | **1.7×** |
| Single invalid | 209 ns | 3,106 ns | **15×** |
| Batch 100 invalid | 20 μs | 280 μs | **14×** |

Validation performance stays consistent on both valid and invalid inputs, with significantly lower memory allocation on failure paths.

### Mapper

| Method | Mean | Allocated |
|--------|-----:|----------:|
| Single source | 17.68 ns | 72 B |
| Two sources | 23.37 ns | 88 B |
| Three sources | 26.15 ns | 88 B |
| Collection (100 items) | 2,026 ns | 9,424 B |

---

## Requirements

- .NET 10.0+

---

## License

MIT
