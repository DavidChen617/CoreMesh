[繁體中文](README.zh-TW.md) | English

# CoreMesh.Validation

A lightweight validation module for `CoreMesh` with a fluent rule API and DI-friendly validator entry point.

## Goals

- Keep validation rules close to the `Command` / `Query` model
- Provide a simple fluent API (`RuleFor(...).NotNull().NotEmpty()...`)
- Support structured validation results and exception-based flow
- Keep the runtime small and predictable

## Design

`CoreMesh.Validation` uses a composition-based design:

1. `IValidatable<T>`
- Implemented by a model (`Command` / `Query`)
- Defines rules via `ConfigureRules(ValidationBuilder<T>)`

2. `ValidationBuilder<T>`
- Builds validation rules with `RuleFor(...)`
- Produces an `ObjectValidator<T>` through `Build()`

3. `ObjectValidator<T>`
- Executes rules and returns `ValidationResult`

4. `Validator<T>`
- DI-friendly validator entry point
- Caches `ObjectValidator<T>` per type for better runtime performance

## Quick Start

### 1. Define a validatable command

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

### 2. Validate via DI (`Validator<T>`)

```csharp
public sealed class CreateProductHandler(IValidator<CreateProductCommand> validator)
{
    public void Handle(CreateProductCommand command)
    {
        validator.ValidateAndThrow(command);
    }
}
```

### 3. Register services

```csharp
builder.Services.AddValidation();
```

## Built-in Rules

- `NotNull()`
- `NotEmpty()` (string only)
- `Length(min, max)` (string only)
- `MinimumLength(min)` (string only)
- `MaximumLength(max)` (string only)
- `Regex(pattern)` (string only)
- `Equal(expected)`
- `Must(predicate, message)`
- `Range(min, max)`
- `GreaterThan(value)`
- `LessThan(value)`
- `WithMessage(message)`

## `WithMessage(...)`

`WithMessage(...)` overrides the error message for the **previous validator** in the same rule chain.

```csharp
builder.RuleFor(x => x.Name)
    .NotEmpty().WithMessage("Name is required.")
    .MinimumLength(2);
```

In the example above, only `NotEmpty()` gets the custom message.

## Validation Result

`Validate(...)` returns a `ValidationResult` with:

- `IsValid`
- `Errors` (`ValidationFailure` list)

```csharp
var result = validator.Validate(command);
if (!result.IsValid)
{
    // handle result.Errors
}
```

You can also throw directly:

```csharp
validator.ValidateAndThrow(command, "Command is invalid.");
```

## Performance Notes

- `Validator<T>` caches built validators (`ObjectValidator<T>`) per type.
- This reduces repeated rule construction overhead at runtime.
- Rule definitions should be static for a given type.

Recommended rule authoring guideline:
- `ConfigureRules(...)` should define rules only, and should not dynamically add/remove rules based on instance values.

## Scope (Current Version)

Included:
- Fluent property rules
- Structured validation result
- Exception-based validation flow (`ValidateAndThrow`)
- DI registration extension (`AddValidation`)

Not included yet:
- Async validation
- Conditional rules (`When/Unless`)
- Rule sets
- Collection rules (`RuleForEach`)
- Localization / error codes / severity levels
- ASP.NET Core automatic model validation integration
