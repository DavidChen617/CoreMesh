[English](README.md) | 繁體中文

# CoreMesh.Validation

`CoreMesh` 的輕量驗證模組，提供 fluent 規則 API 與可透過 DI 注入的驗證入口。

## 目標

- 讓驗證規則靠近 `Command` / `Query` 模型
- 提供簡單直觀的 fluent API（`RuleFor(...).NotNull().NotEmpty()...`）
- 支援結構化驗證結果與例外流程
- 維持小而可預期的執行成本

## 設計

`CoreMesh.Validation` 採用組合式設計：

1. `IValidatable<T>`
- 由模型（`Command` / `Query`）實作
- 透過 `ConfigureRules(ValidationBuilder<T>)` 定義規則

2. `ValidationBuilder<T>`
- 使用 `RuleFor(...)` 建構規則
- 透過 `Build()` 產生 `ObjectValidator<T>`

3. `ObjectValidator<T>`
- 執行規則並回傳 `ValidationResult`

4. `Validator<T>`
- 可透過 DI 注入的驗證入口
- 會依型別快取 `ObjectValidator<T>`，降低執行期重複建規則成本

## 快速開始

### 1. 定義可驗證的 Command

```csharp
using CoreMesh.Validation;

public sealed record CreateProductCommand(string? Name, decimal Price, string? Description)
    : IValidatable<CreateProductCommand>
{
    public void ConfigureRules(ValidationBuilder<CreateProductCommand> builder)
    {
        builder.RuleFor(x => x.Name)
            .NotNull()
            .NotEmpty()
            .Length(2, 50);

        builder.RuleFor(x => x.Description)
            .NotNull()
            .NotEmpty();

        builder.RuleFor(x => x.Price)
            .GreaterThan(0m);
    }
}
```

### 2. 透過 DI 使用 `Validator<T>`

```csharp
public sealed class CreateProductHandler(IValidator<CreateProductCommand> validator)
{
    public void Handle(CreateProductCommand command)
    {
        validator.ValidateAndThrow(command);
    }
}
```

### 3. 註冊服務

```csharp
builder.Services.AddValidation();
```

## 內建規則

- `NotNull()`
- `NotEmpty()`（僅字串）
- `Length(min, max)`（僅字串）
- `MinimumLength(min)`（僅字串）
- `MaximumLength(max)`（僅字串）
- `Regex(pattern)`（僅字串）
- `Equal(expected)`
- `Must(predicate, message)`
- `Range(min, max)`
- `GreaterThan(value)`
- `LessThan(value)`
- `WithMessage(message)`

## `WithMessage(...)`

`WithMessage(...)` 會覆蓋同一條 rule chain 中「前一個 validator」的錯誤訊息。

```csharp
builder.RuleFor(x => x.Name)
    .NotEmpty().WithMessage("Name is required.")
    .MinimumLength(2);
```

以上例子中，只有 `NotEmpty()` 會使用自訂訊息。

## 驗證結果

`Validate(...)` 會回傳 `ValidationResult`，包含：

- `IsValid`
- `Errors`（`ValidationFailure` 清單）

```csharp
var result = validator.Validate(command);
if (!result.IsValid)
{
    // 處理 result.Errors
}
```

也可以直接丟例外：

```csharp
validator.ValidateAndThrow(command, "Command is invalid.");
```

## 效能說明

- `Validator<T>` 會依型別快取已建立的 `ObjectValidator<T>`
- 可降低重複建構規則的執行期成本
- 規則定義應保持為型別層級固定規則（static rules）

建議規範：
- `ConfigureRules(...)` 應只負責定義規則，不應根據 instance 當前值動態增減規則。

## 範圍（目前版本）

已包含：
- Fluent 屬性規則
- 結構化驗證結果
- 例外型驗證流程（`ValidateAndThrow`）
- DI 註冊擴充方法（`AddValidation`）

尚未包含：
- 非同步驗證
- 條件規則（`When/Unless`）
- RuleSet
- 集合規則（`RuleForEach`）
- 多語系 / 錯誤碼 / 嚴重度
- ASP.NET Core 自動模型驗證整合
