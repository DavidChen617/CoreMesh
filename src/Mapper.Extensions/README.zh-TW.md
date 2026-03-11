[English](README.md) | 繁體中文

# CoreMesh.Mapper.Extensions

`CoreMesh.Mapper` 的 DI 註冊擴充。

## 使用方式

```csharp
using CoreMesh.Mapper.Extensions;

builder.Services.AddCoreMeshMapper(typeof(Program).Assembly);
```

以 singleton 方式註冊 `IMapper`，並掃描指定 assembly 中的 mapping contract（`IMapFrom<...>`、`IMapWith<...>`）。可一次傳入多個 assembly；重複呼叫為 no-op（`TryAddSingleton` 語意）。

定義 mapping contract 與使用 `IMapper` 的方式請參考 `CoreMesh.Mapper`。
