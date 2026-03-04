[English](README.md) | 繁體中文

# CoreMesh.Validation

`CoreMesh` 的輕量驗證模組，提供 fluent 規則 API 與可透過 DI 注入的驗證入口。

## 目標

- 讓驗證規則靠近 `Command` / `Query` 模型
- 提供簡單直觀的 fluent API（`For(...).NotNull().NotEmpty()...`）
- 支援結構化驗證結果
- 維持小而可預期的執行成本

## 設計

`CoreMesh.Validation` 採用組合式設計：

1. `IValidatable<T>`
   - 由模型本身實作，或獨立的 Validator 類別實作
   - 透過 `ConfigureValidateRules(ValidationBuilder<T>)` 定義規則

2. `ValidationBuilder<T>`
   - 使用 `For(...)` 建構規則
   - 支援鏈式呼叫各種驗證方法

3. `IValidator` / `Validator`
   - 可透過 DI 注入的驗證入口
   - 會依型別快取規則，降低執行期重複建規則成本
   - 支援兩種模式：model 自己實現 `IValidatable<T>` 或從 DI 取得獨立的 validator

## 快速開始

### 方式一：Model 自己實現 `IValidatable<T>`

```csharp
using CoreMesh.Validation;
using CoreMesh.Validation.Extensions;

public sealed record CreateProductCommand(string? Name, decimal Price, string? Description)
    : IValidatable<CreateProductCommand>
{
    public void ConfigureValidateRules(ValidationBuilder<CreateProductCommand> builder)
    {
        builder.For(x => x.Name)
            .NotNull()
            .NotEmpty()
            .MinLength(2)
            .MaxLength(50);

        builder.For(x => x.Description)
            .NotNull()
            .NotEmpty();

        builder.For(x => x.Price)
            .GreaterThan(0m);
    }
}
```

### 方式二：獨立的 Validator 類別（關注點分離）

```csharp
// Model（純資料）
public sealed record CreateProductCommand(string? Name, decimal Price, string? Description);

// Validator（獨立類別）
public sealed class CreateProductCommandValidator : IValidatable<CreateProductCommand>
{
    public void ConfigureValidateRules(ValidationBuilder<CreateProductCommand> builder)
    {
        builder.For(x => x.Name)
            .NotNull()
            .NotEmpty()
            .MinLength(2)
            .MaxLength(50);

        builder.For(x => x.Price)
            .GreaterThan(0m);
    }
}
```

### 使用 `IValidator`

```csharp
public sealed class CreateProductHandler(IValidator validator)
{
    public void Handle(CreateProductCommand command)
    {
        var result = validator.Validate(command);
        if (!result.IsValid)
        {
            // 處理驗證錯誤
            throw new InvalidOperationException(
                string.Join(", ", result.Errors.SelectMany(e => e.Value)));
        }
    }
}
```

### 註冊服務

```csharp
using CoreMesh.Validation.Extensions;

// 掃描 assembly 註冊所有 IValidatable<T> 實作
builder.Services.AddValidatable(typeof(Program).Assembly);
```

## 內建規則

### 通用規則
- `Must(predicate, message)` - 自訂驗證邏輯
- `StopOnInvalid()` - 若前面的規則失敗，停止後續規則檢查

### 字串規則
- `NotNull(message?)` - 不可為 null
- `NotEmpty(message?)` - 不可為空白
- `MinLength(min, message?)` - 最小長度
- `MaxLength(max, message?)` - 最大長度
- `EmailAddress(message?)` - Email 格式

### 數值比較規則
- `GreaterThan(value, message?)` - 大於
- `GreaterThanOrEqual(value, message?)` - 大於等於
- `LessThan(value, message?)` - 小於
- `LessThanOrEqual(value, message?)` - 小於等於

### 集合規則
- `NotNull(message?)` - 集合不可為 null
- `NotEmpty(message?)` - 集合不可為空

## 自訂錯誤訊息

每個驗證方法都支援選填的 `message` 參數：

```csharp
builder.For(x => x.Name)
    .NotEmpty("名稱為必填欄位")
    .MinLength(2, "名稱至少需要 2 個字元");
```

## `StopOnInvalid()` 短路驗證

當某個屬性驗證失敗時，可以使用 `StopOnInvalid()` 停止該屬性後續的規則檢查：

```csharp
builder.For(x => x.Name)
    .NotNull()
    .StopOnInvalid()  // 若 NotNull 失敗，不會執行 MinLength
    .MinLength(5);
```

## 驗證結果

`Validate(...)` 會回傳 `ValidationResult`，包含：

- `IsValid` - 是否通過驗證
- `Errors` - `Dictionary<string, List<string>>`，key 為屬性名稱，value 為錯誤訊息清單

```csharp
var result = validator.Validate(command);
if (!result.IsValid)
{
    foreach (var (property, messages) in result.Errors)
    {
        Console.WriteLine($"{property}: {string.Join(", ", messages)}");
    }
}
```

## 效能說明

- `Validator` 會依型別快取已建立的規則
- 可降低重複建構規則的執行期成本
- 規則定義應保持為型別層級固定規則（static rules）

建議規範：
- `ConfigureValidateRules(...)` 應只負責定義規則，不應根據 instance 當前值動態增減規則

## Benchmark

與 FluentValidation 的效能對比：

| 情境 | CoreMesh | FluentValidation | 速度提升 | 記憶體節省 |
|------|----------|------------------|----------|------------|
| 單筆驗證（有效） | 204 ns | 343 ns | **1.7x** | **63%** |
| 單筆驗證（無效） | 192 ns | 2,967 ns | **15x** | **92%** |
| 批量 100 筆（有效） | 22 μs | 35 μs | **1.6x** | **63%** |
| 批量 100 筆（無效） | 20 μs | 283 μs | **14x** | **92%** |

<details>
<summary>完整 BenchmarkDotNet 結果</summary>

```
| Method                                           | Mean         | Error       | StdDev      | Gen0     | Gen1    | Allocated |
|------------------------------------------------- |-------------:|------------:|------------:|---------:|--------:|----------:|
| CoreMesh_Validate_Valid                          |     204.1 ns |     1.00 ns |     0.84 ns |   0.0286 |       - |     240 B |
| FluentValidation_Validate_Valid                  |     343.4 ns |     3.18 ns |     2.82 ns |   0.0792 |       - |     664 B |
| CoreMesh_Validate_Invalid                        |     192.2 ns |     3.77 ns |     3.53 ns |   0.0763 |       - |     640 B |
| FluentValidation_Validate_Invalid                |   2,966.6 ns |    29.51 ns |    24.64 ns |   1.0223 |  0.0114 |    8552 B |
| CoreMesh_Validate_Collection_Valid_100           |  22,176.2 ns |   124.94 ns |   104.33 ns |   2.9602 |  0.0916 |   24992 B |
| FluentValidation_Validate_Collection_Valid_100   |  34,858.8 ns |   438.61 ns |   366.26 ns |   8.0566 |  0.1831 |   67392 B |
| CoreMesh_Validate_Collection_Invalid_100         |  20,172.7 ns |   393.57 ns |   453.24 ns |   7.7515 |  1.1597 |   64904 B |
| FluentValidation_Validate_Collection_Invalid_100 | 283,131.8 ns | 5,164.64 ns | 4,578.32 ns | 102.0508 | 38.0859 |  854248 B |
```

</details>

**關鍵發現：**
- **驗證失敗時差距最大** - FluentValidation 處理失敗情況時效能驟降，CoreMesh 則保持穩定
- **記憶體分配差距懸殊** - 無效資料時 FluentValidation 分配 8.5KB vs CoreMesh 640B
- **批量處理無效資料** - FluentValidation 分配 854KB vs CoreMesh 65KB

## 範圍（目前版本）

已包含：
- Fluent 屬性規則
- 結構化驗證結果
- 短路驗證（`StopOnInvalid`）
- DI 註冊擴充方法（`AddValidatable`）
- 兩種使用模式（model 實作或獨立 validator）

尚未包含：
- 非同步驗證
- 條件規則（`When/Unless`）
- RuleSet
- 集合規則（`RuleForEach`）
- 多語系 / 錯誤碼 / 嚴重度
- ASP.NET Core 自動模型驗證整合