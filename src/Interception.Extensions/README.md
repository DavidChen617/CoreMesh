English | [繁體中文](README.zh-TW.md)

# CoreMesh.Interception.Extensions

DI registration extension for `CoreMesh.Interception`. Scans assemblies for interceptors and `[InterceptedBy<T>]`-decorated interfaces, then wraps registered services with the appropriate proxy.

## Usage

```csharp
using CoreMesh.Interception.Extensions;

builder.Services.AddSingleton<IMyService, MyService>();
builder.Services.AddInterceptor(typeof(Program).Assembly);
```

## How `AddInterceptor` Works

1. Finds all `IInterceptor` / `IAsyncInterceptor` implementations and registers them as keyed singletons.
2. Finds all interfaces decorated with `[InterceptedBy<T>]` (including attributes inherited from base interfaces).
3. Rewraps each matched service registration with `InterceptorProxy<T>` or `AsyncInterceptorProxy<T>`.
   - If any interceptor is async → `AsyncInterceptorProxy<T>` is used.
   - If multiple interceptors are declared → combined into `CompositeInterceptor`.

## Notes

- The service must already be registered before calling `AddInterceptor`.
- The proxy lifetime matches the original service's lifetime.
- See `CoreMesh.Interception` for defining interceptors and creating proxies manually.
