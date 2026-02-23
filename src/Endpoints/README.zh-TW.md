[English](README.md) | 繁體中文

# CoreMesh.Endpoints

`CoreMesh.Endpoints` 提供一個輕量的 ASP.NET Core Endpoint 掃描與路由掛載封裝。

它可以幫你：

1. 用 class 方式定義 endpoint
2. 透過掃描註冊 endpoint 類型
3. 集中掛載一般 endpoint 與 grouped endpoint

## 功能

- `IEndpoint`：註冊 root endpoint
- `IGroupEndpoint`：定義 route group 與共用設定
- `IGroupedEndpoint<TGroup>`：把 endpoint 掛在指定 group 下面
- `AddEndpoints()`：掃描已載入 assemblies 並註冊 endpoint 類型
- `MapEndpoints()`：把註冊好的 endpoint 掛到 `WebApplication`

## 安裝與使用

引用專案/套件後，在 `Program.cs` 呼叫：

```csharp
using CoreMesh.Endpoints.Extensions;

builder.Services.AddEndpoints();

var app = builder.Build();

app.MapEndpoints();
```

## 核心介面

### `IEndpoint`

用來註冊 root endpoint：

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

用來建立 route group 並掛載群組內 endpoint：

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

## 設計說明（目前版本）

- Endpoint 掃描使用 `AppDomain.CurrentDomain.GetAssemblies()`
- Endpoint 目前使用 `Singleton` 註冊
- 掛載邏輯透過實作介面 (`IEndpoint`, `IGroupEndpoint`, `IGroupedEndpoint`) 進行約定式配對

這些行為目前刻意先維持與原始版本一致，後續迭代再收斂（例如改成顯式 assembly 掃描與生命週期調整）。

## 測試覆蓋

目前測試包含：

1. `AddEndpoints()` 對 endpoint / group / grouped endpoint 的註冊
2. `MapEndpoints()` 對 `AddRoute()` 與 group 配對行為的驗證

