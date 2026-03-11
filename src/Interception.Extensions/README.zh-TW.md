[English](README.md) | 繁體中文

# CoreMesh.Interception.Extensions

`CoreMesh.Interception` 的 DI 註冊擴充。掃描 assembly 中的 interceptor 與帶有 `[InterceptedBy<T>]` 的 interface，自動將已註冊的服務包裝成對應的 proxy。

## 使用方式

```csharp
using CoreMesh.Interception.Extensions;

builder.Services.AddSingleton<IMyService, MyService>();
builder.Services.AddInterceptor(typeof(Program).Assembly);
```

## `AddInterceptor` 運作步驟

1. 找出所有 `IInterceptor` / `IAsyncInterceptor` 實作，以 keyed singleton 方式註冊。
2. 找出所有帶有 `[InterceptedBy<T>]` 的 interface（含從父 interface 繼承的 attribute）。
3. 將每個匹配的服務 registration 替換為 `InterceptorProxy<T>` 或 `AsyncInterceptorProxy<T>`。
   - 若任一 interceptor 為非同步 → 使用 `AsyncInterceptorProxy<T>`。
   - 若宣告多個 interceptor → 合併為 `CompositeInterceptor`。

## 注意事項

- 服務必須在呼叫 `AddInterceptor` 之前已完成註冊。
- Proxy 的 lifetime 與原本服務的 lifetime 相同。
- 若需自訂 interceptor 或手動建立 proxy，請參考 `CoreMesh.Interception`。
