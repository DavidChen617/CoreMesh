# CoreMesh.Mapper

`CoreMesh.Mapper` 是一個輕量、以慣例為主的物件映射工具，適合用在應用層 DTO 與 Entity 的映射。

它會從型別實作的映射介面中探索規則，並在註冊階段編譯委派供執行期使用。

## 功能特色

- 透過 Assembly 掃描進行慣例註冊
- 強型別映射 API
- 單向映射契約：`IMapFrom<TSource, TDestination>`
- 雙向映射契約：`IMapWith<TEntity, TSelf>`
- DI 註冊擴充方法：`AddCoreMeshMapper(...)`

## 映射契約

### `IMapFrom<TSource, TDestination>`

用於單向映射。

```csharp
using CoreMesh.Mapper;

public sealed record UserDto(string FullName, string Email)
    : IMapFrom<User, UserDto>
{
    public UserDto MapFrom(User source)
        => new($"{source.FirstName} {source.LastName}", source.Email);
}
```

### `IMapWith<TEntity, TSelf>`

用於雙向映射（entity <-> dto）。

```csharp
using CoreMesh.Mapper;

public sealed record UserDto(string FullName, string Email)
    : IMapWith<User, UserDto>
{
    public UserDto() : this(string.Empty, string.Empty) { }

    public UserDto MapFrom(User entity)
        => new($"{entity.FirstName} {entity.LastName}".Trim(), entity.Email);

    public User MapTo()
    {
        var parts = FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return new User
        {
            Id = string.Empty,
            FirstName = parts.Length > 0 ? parts[0] : string.Empty,
            LastName = parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : string.Empty,
            Email = Email,
            Password = string.Empty
        };
    }
}
```

### 多來源單向映射（`IMapFrom<...>`）

對於組合型 DTO 的場景，`CoreMesh.Mapper` 支援最多三個來源物件的單向映射。

#### 兩個來源

```csharp
using CoreMesh.Mapper;

public sealed record UserAggregateDto(string FullName, string Email, string City)
    : IMapFrom<User, UserProfile, UserAggregateDto>
{
    public UserAggregateDto() : this(string.Empty, string.Empty, string.Empty) { }

    public UserAggregateDto MapFrom(User user, UserProfile profile)
        => new($"{user.FirstName} {user.LastName}", user.Email, profile.City);
}
```

#### 三個來源

```csharp
using CoreMesh.Mapper;

public sealed record UserSummaryDto(string FullName, string Email, string City, int OrderCount)
    : IMapFrom<User, UserProfile, UserStats, UserSummaryDto>
{
    public UserSummaryDto() : this(string.Empty, string.Empty, string.Empty, 0) { }

    public UserSummaryDto MapFrom(User user, UserProfile profile, UserStats stats)
        => new($"{user.FirstName} {user.LastName}", user.Email, profile.City, stats.OrderCount);
}
```

## 使用方式（不透過 DI）

```csharp
using CoreMesh.Mapper;

var mapper = new Mapper();
mapper.RegisterMapper(typeof(Program).Assembly);

var dto = mapper.Map<User, UserDto>(user);
var users = mapper.Map<User, UserDto>(userList).ToList();

var aggregate = mapper.Map<User, UserProfile, UserAggregateDto>(user, profile);
var summary = mapper.Map<User, UserProfile, UserStats, UserSummaryDto>(user, profile, stats);
```

## 使用方式（透過 DI）

```csharp
using CoreMesh.Mapper;
using CoreMesh.Mapper.Extensions;

builder.Services.AddCoreMeshMapper(typeof(Program).Assembly);
```

取得服務並使用：

```csharp
var mapper = serviceProvider.GetRequiredService<IMapper>();
var dto = mapper.Map<User, UserDto>(user);
```

## 公開 API

- `IMapFrom<TSource, TDestination>`
- `IMapFrom<TSource1, TSource2, TDestination>`
- `IMapFrom<TSource1, TSource2, TSource3, TDestination>`
- `IMapWith<TEntity, TSelf>`
- `IMapper`
- `Mapper`

## 注意事項

- `IMapWith<TEntity, TSelf>` 的註冊要求實作型別必須提供 **public 無參數建構式**。
- 映射註冊目前使用 `TryAdd`，若同一組 `(source, destination)` 被重複註冊，會保留第一個註冊結果。
- 集合映射方法回傳的是延遲執行序列。

## 目前範圍

此模組目前刻意聚焦於：

- 強型別物件映射
- 強型別多來源單向映射（最多 3 個來源）
- Assembly 掃描註冊
- 慣例式委派編譯

目前不包含：

- 弱型別 runtime 映射 API
- 自動巢狀物件圖映射
- 屬性逐一反射映射
- 類似 AutoMapper 的 Profile / 設定 DSL
