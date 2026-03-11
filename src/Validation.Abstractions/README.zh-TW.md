[English](README.md) | 繁體中文

# CoreMesh.Validation.Abstractions

CoreMesh 驗證系統的 contract 與內建規則擴充，不依賴任何 DI 或框架。

一般應用程式專案建議直接使用 `CoreMesh.Validation`（已包含這些 abstractions 以及 `Validator` 實作與 DI 註冊）。

## Contracts

| 型別 | 說明 |
|------|------|
| `IValidator` | 入口點：`Validate<T>(model)` |
| `IValidatable<T>` | 由 model 或 validator 類別實作，定義驗證規則 |
| `IValidationBuilder<T>` | 提供 `For(expression)` 以開始規則鏈 |
| `ValidationResult` | `IsValid` + `Errors`（以屬性名稱為 key 的 dictionary） |
| `RuleResult` | 單一規則結果，含可選錯誤訊息與 `StopOnError` 旗標 |

## 內建規則

```csharp
builder.For(x => x.Name)
    .NotNull()
    .StopOnInvalid()   // 短路：前面規則失敗時停止後續規則
    .NotEmpty()
    .MinLength(2)
    .MaxLength(50);

builder.For(x => x.Email).EmailAddress();
builder.For(x => x.Age).GreaterThan(0);
builder.For(x => x.Score).LessThanOrEqual(100);
```

| 類別 | 規則 |
|------|------|
| 通用 | `Must(predicate, msg?)`, `StopOnInvalid()` |
| 字串 | `NotNull`, `NotEmpty`, `MinLength`, `MaxLength`, `EmailAddress` |
| 比較 | `GreaterThan`, `GreaterThanOrEqual`, `LessThan`, `LessThanOrEqual` |
| 集合 | `NotNull`, `NotEmpty`（用於 `List<T>?`） |

`NotNull`、`GreaterThanOrEqual`、`LessThanOrEqual` 另提供 nullable value type 多載。
