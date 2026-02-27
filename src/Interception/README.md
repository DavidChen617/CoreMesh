# CoreMesh.Interception

`CoreMesh.Interception` is a lightweight AOP (Aspect-Oriented Programming) interception framework built on `DispatchProxy`.

It provides method interception capabilities for interfaces, supporting both synchronous and asynchronous interceptors.

## Features

- Dynamic proxy generation via `DispatchProxy`
- Synchronous interceptor: `IInterceptor` (void methods)
- Asynchronous interceptor: `IAsyncInterceptor` (ValueTask methods)
- Support for `Task`, `Task<T>`, `ValueTask`, `ValueTask<T>` return types
- Multiple interceptors via `CompositeInterceptor`
- Declarative binding via `[InterceptedBy<T>]` attribute
- DI registration extension: `AddInterceptor(...)`

## Interceptor Contracts

### `IInterceptor`

Use this contract for synchronous interception logic.

```csharp
using System.Reflection;
using CoreMesh.Interception;

public class LoggingInterceptor : IInterceptor
{
    public void BeforeInvoke(MethodInfo method, object?[]? args)
        => Console.WriteLine($"Before: {method.Name}");

    public void AfterInvoke(MethodInfo method, object?[]? args, object? result)
        => Console.WriteLine($"After: {method.Name} => {result}");

    public void OnError(MethodInfo method, object?[]? args, Exception error)
        => Console.WriteLine($"Error: {method.Name} - {error.Message}");
}
```

### `IAsyncInterceptor`

Use this contract when interceptor logic requires async operations.

```csharp
using System.Reflection;
using CoreMesh.Interception;

public class AsyncLoggingInterceptor : IAsyncInterceptor
{
    public async ValueTask BeforeInvoke(MethodInfo method, object?[]? args)
    {
        await LogAsync($"Before: {method.Name}");
    }

    public async ValueTask AfterInvoke(MethodInfo method, object?[]? args, object? result)
    {
        await LogAsync($"After: {method.Name} => {result}");
    }

    public async ValueTask OnError(MethodInfo method, object?[]? args, Exception error)
    {
        await LogAsync($"Error: {method.Name} - {error.Message}");
    }

    private Task LogAsync(string message) => Task.CompletedTask;
}
```

## Declarative Interceptor Binding

Use `[InterceptedBy<T>]` attribute on interfaces to declare interceptors.

```csharp
using CoreMesh.Interception;

[InterceptedBy<LoggingInterceptor>]
[InterceptedBy<CacheInterceptor>]
public interface IMyService
{
    Task<string> GetDataAsync(int id);
}

public class MyService : IMyService
{
    public async Task<string> GetDataAsync(int id)
    {
        await Task.Delay(100);
        return $"Data-{id}";
    }
}
```

### Inherited Interceptors

Interceptors can be inherited from base interfaces.

```csharp
[InterceptedBy<LoggingInterceptor>]
public interface ILoggable;

public interface IMyService : ILoggable
{
    Task DoWorkAsync();
}
```

## Usage (Without DI)

```csharp
using CoreMesh.Interception;

var service = new MyService();
var interceptor = new LoggingInterceptor();

// Create proxy with sync interceptor
var proxy = InterceptorProxy<IMyService>.Create(service, interceptor);
var result = await proxy.GetDataAsync(1);

// Create proxy with async interceptor
var asyncInterceptor = new AsyncLoggingInterceptor();
var asyncProxy = AsyncInterceptorProxy<IMyService>.Create(service, asyncInterceptor);
```

### Multiple Interceptors

```csharp
using CoreMesh.Interception;

var composite = new CompositeInterceptor(
    new LoggingInterceptor(),
    new CacheInterceptor(),
    new MetricsInterceptor()
);

var proxy = InterceptorProxy<IMyService>.Create(service, composite);
```

## Usage (With DI)

```csharp
using CoreMesh.Interception.Extensions;

// Register service and interceptors
builder.Services.AddSingleton<IMyService, MyService>();
builder.Services.AddInterceptor(typeof(Program).Assembly);
```

Resolve and use:

```csharp
var service = serviceProvider.GetRequiredService<IMyService>();
var result = await service.GetDataAsync(1); // Intercepted!
```

## Public API

- `IInterceptorBase` - Base marker interface
- `IInterceptor` - Synchronous interceptor contract
- `IAsyncInterceptor` - Asynchronous interceptor contract
- `InterceptorProxy<T>` - Sync proxy using `IInterceptor`
- `AsyncInterceptorProxy<T>` - Async proxy using `IAsyncInterceptor`
- `CompositeInterceptor` - Combines multiple interceptors
- `InterceptedByAttribute<T>` - Declarative interceptor binding
- `AddInterceptor(...)` - DI registration extension

## Performance

Benchmark results (sync method, empty interceptor):

| Method | Mean | Allocated |
|--------|------|-----------|
| Direct | ~2 ns | - |
| InterceptorProxy | ~180 ns | - |
| AsyncInterceptorProxy | ~246 ns | - |
| Castle DynamicProxy | ~104 ns | - |

## Notes

- Interceptors are registered as keyed singletons in DI.
- If any interceptor is async, `AsyncInterceptorProxy` is used automatically.
- `CompositeInterceptor` implements both `IInterceptor` and `IAsyncInterceptor`.
- When using `IInterceptor` interface, async interceptors will block via `GetAwaiter().GetResult()`.

## Current Scope

This module intentionally focuses on:

- Interface-based method interception
- DispatchProxy-based dynamic proxy
- Sync and async interceptor support
- DI integration with attribute-based binding

It does not currently include:

- Class-based interception (requires IL weaving)
- Constructor interception
- Property/field interception
- Compile-time weaving (e.g., Fody)
