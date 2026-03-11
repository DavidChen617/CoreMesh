English | [繁體中文](README.zh-TW.md)

# CoreMesh.Validation.Abstractions

Contracts and built-in rule extensions for the CoreMesh validation system. No DI or framework dependencies.

For most application projects, use `CoreMesh.Validation` instead (it includes these abstractions plus the `Validator` implementation and DI registration).

## Contracts

| Type | Description |
|------|-------------|
| `IValidator` | Entry point: `Validate<T>(model)` |
| `IValidatable<T>` | Implemented by models or validator classes to define rules |
| `IValidationBuilder<T>` | Exposes `For(expression)` to start a rule chain |
| `ValidationResult` | `IsValid` + `Errors` (dictionary keyed by property name) |
| `RuleResult` | Single rule outcome with optional error message and `StopOnError` flag |

## Built-in Rules

```csharp
builder.For(x => x.Name)
    .NotNull()
    .StopOnInvalid()   // short-circuit: stop if any previous rule failed
    .NotEmpty()
    .MinLength(2)
    .MaxLength(50);

builder.For(x => x.Email).EmailAddress();
builder.For(x => x.Age).GreaterThan(0);
builder.For(x => x.Score).LessThanOrEqual(100);
```

| Category | Rules |
|----------|-------|
| General | `Must(predicate, msg?)`, `StopOnInvalid()` |
| String | `NotNull`, `NotEmpty`, `MinLength`, `MaxLength`, `EmailAddress` |
| Comparison | `GreaterThan`, `GreaterThanOrEqual`, `LessThan`, `LessThanOrEqual` |
| Collection | `NotNull`, `NotEmpty` (for `List<T>?`) |

Nullable value type overloads are available for `NotNull`, `GreaterThanOrEqual`, and `LessThanOrEqual`.
