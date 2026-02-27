# CoreMesh.Interception

`CoreMesh.Interception` 是一個基於 `DispatchProxy` 的輕量級 AOP（面向切面程式設計）攔截框架。

它為介面提供方法攔截功能，同時支援同步和非同步攔截器。

## 功能特色

- 透過 `DispatchProxy` 動態產生代理
- 同步攔截器：`IInterceptor`（void 方法）
- 非同步攔截器：`IAsyncInterceptor`（ValueTask 方法）
- 支援 `Task`、`Task<T>`、`ValueTask`、`ValueTask<T>` 回傳型別
- 透過 `CompositeInterceptor` 組合多個攔截器
- 透過 `[InterceptedBy<T>]` 屬性宣告式綁定
- DI 註冊擴充方法：`AddInterceptor(...)`

## 攔截器契約

### `IInterceptor`

用於同步攔截邏輯。

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

當攔截邏輯需要非同步操作時使用。

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

## 宣告式攔截器綁定

在介面上使用 `[InterceptedBy<T>]` 屬性來宣告攔截器。

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

### 繼承攔截器

攔截器可以從基底介面繼承。

```csharp
[InterceptedBy<LoggingInterceptor>]
public interface ILoggable;

public interface IMyService : ILoggable
{
    Task DoWorkAsync();
}
```

## 使用方式（不透過 DI）

```csharp
using CoreMesh.Interception;

var service = new MyService();
var interceptor = new LoggingInterceptor();

// 使用同步攔截器建立代理
var proxy = InterceptorProxy<IMyService>.Create(service, interceptor);
var result = await proxy.GetDataAsync(1);

// 使用非同步攔截器建立代理
var asyncInterceptor = new AsyncLoggingInterceptor();
var asyncProxy = AsyncInterceptorProxy<IMyService>.Create(service, asyncInterceptor);
```

### 多個攔截器

```csharp
using CoreMesh.Interception;

var composite = new CompositeInterceptor(
    new LoggingInterceptor(),
    new CacheInterceptor(),
    new MetricsInterceptor()
);

var proxy = InterceptorProxy<IMyService>.Create(service, composite);
```

## 使用方式（透過 DI）

```csharp
using CoreMesh.Interception.Extensions;

// 註冊服務和攔截器
builder.Services.AddSingleton<IMyService, MyService>();
builder.Services.AddInterceptor(typeof(Program).Assembly);
```

取得服務並使用：

```csharp
var service = serviceProvider.GetRequiredService<IMyService>();
var result = await service.GetDataAsync(1); // 已被攔截！
```

## 公開 API

- `IInterceptorBase` - 基底標記介面
- `IInterceptor` - 同步攔截器契約
- `IAsyncInterceptor` - 非同步攔截器契約
- `InterceptorProxy<T>` - 使用 `IInterceptor` 的同步代理
- `AsyncInterceptorProxy<T>` - 使用 `IAsyncInterceptor` 的非同步代理
- `CompositeInterceptor` - 組合多個攔截器
- `InterceptedByAttribute<T>` - 宣告式攔截器綁定
- `AddInterceptor(...)` - DI 註冊擴充方法

## 效能

基準測試結果（同步方法，空攔截器）：

| 方法 | 平均耗時 | 記憶體配置 |
|------|---------|-----------|
| 直接呼叫 | ~2 ns | - |
| InterceptorProxy | ~180 ns | - |
| AsyncInterceptorProxy | ~246 ns | - |
| Castle DynamicProxy | ~104 ns | - |

## 注意事項

- 攔截器在 DI 中以 keyed singleton 方式註冊。
- 若任一攔截器為非同步，會自動使用 `AsyncInterceptorProxy`。
- `CompositeInterceptor` 同時實作 `IInterceptor` 和 `IAsyncInterceptor`。
- 當使用 `IInterceptor` 介面時，非同步攔截器會透過 `GetAwaiter().GetResult()` 阻塞等待。

## 目前範圍

此模組目前刻意聚焦於：

- 基於介面的方法攔截
- 基於 DispatchProxy 的動態代理
- 同步與非同步攔截器支援
- 與 DI 整合的屬性綁定

目前不包含：

- 類別攔截（需要 IL 織入）
- 建構子攔截
- 屬性/欄位攔截
- 編譯期織入（如 Fody）
