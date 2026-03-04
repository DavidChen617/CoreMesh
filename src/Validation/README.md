[繁體中文](README.zh-TW.md) | English

# CoreMesh.Validation

A lightweight validation module for `CoreMesh` with a fluent rule API and DI-friendly validator entry point.

## Goals

- Keep validation rules close to the `Command` / `Query` model
- Provide a simple fluent API (`For(...).NotNull().NotEmpty()...`)
- Support structured validation results
- Keep the runtime small and predictable

## Design

`CoreMesh.Validation` uses a composition-based design:

1. `IValidatable<T>`
   - Implemented by the model itself, or by a separate validator class
   - Defines rules via `ConfigureValidateRules(ValidationBuilder<T>)`

2. `ValidationBuilder<T>`
   - Builds validation rules with `For(...)`
   - Supports fluent chaining of validation methods

3. `IValidator` / `Validator`
   - DI-friendly validator entry point
   - Caches rules per type for better runtime performance
   - Supports two modes: model implements `IValidatable<T>` or separate validator from DI

## Quick Start

### Option 1: Model implements `IValidatable<T>`

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

### Option 2: Separate Validator Class (Separation of Concerns)

```csharp
// Model (pure data)
public sealed record CreateProductCommand(string? Name, decimal Price, string? Description);

// Validator (separate class)
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

### Using `IValidator`

```csharp
public sealed class CreateProductHandler(IValidator validator)
{
    public void Handle(CreateProductCommand command)
    {
        var result = validator.Validate(command);
        if (!result.IsValid)
        {
            // Handle validation errors
            throw new InvalidOperationException(
                string.Join(", ", result.Errors.SelectMany(e => e.Value)));
        }
    }
}
```

### Register Services

```csharp
using CoreMesh.Validation.Extensions;

// Scan assembly and register all IValidatable<T> implementations
builder.Services.AddValidatable(typeof(Program).Assembly);
```

## Built-in Rules

### General Rules
- `Must(predicate, message)` - Custom validation logic
- `StopOnInvalid()` - Stop subsequent rules if previous rules failed

### String Rules
- `NotNull(message?)` - Must not be null
- `NotEmpty(message?)` - Must not be empty or whitespace
- `MinLength(min, message?)` - Minimum length
- `MaxLength(max, message?)` - Maximum length
- `EmailAddress(message?)` - Email format

### Comparison Rules
- `GreaterThan(value, message?)` - Greater than
- `GreaterThanOrEqual(value, message?)` - Greater than or equal
- `LessThan(value, message?)` - Less than
- `LessThanOrEqual(value, message?)` - Less than or equal

### Collection Rules
- `NotNull(message?)` - Collection must not be null
- `NotEmpty(message?)` - Collection must not be empty

## Custom Error Messages

Each validation method supports an optional `message` parameter:

```csharp
builder.For(x => x.Name)
    .NotEmpty("Name is required")
    .MinLength(2, "Name must be at least 2 characters");
```

## `StopOnInvalid()` Short-Circuit Validation

Use `StopOnInvalid()` to stop subsequent rule checks for a property when validation fails:

```csharp
builder.For(x => x.Name)
    .NotNull()
    .StopOnInvalid()  // If NotNull fails, MinLength won't execute
    .MinLength(5);
```

## Validation Result

`Validate(...)` returns a `ValidationResult` with:

- `IsValid` - Whether validation passed
- `Errors` - `Dictionary<string, List<string>>`, key is property name, value is list of error messages

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

## Performance Notes

- `Validator` caches built rules per type
- This reduces repeated rule construction overhead at runtime
- Rule definitions should be static for a given type

Recommended rule authoring guideline:
- `ConfigureValidateRules(...)` should define rules only, and should not dynamically add/remove rules based on instance values

## Benchmark

Performance comparison with FluentValidation:

| Scenario | CoreMesh | FluentValidation | Speedup | Memory Saved |
|----------|----------|------------------|---------|--------------|
| Single (Valid) | 204 ns | 343 ns | **1.7x** | **63%** |
| Single (Invalid) | 192 ns | 2,967 ns | **15x** | **92%** |
| Batch 100 (Valid) | 22 μs | 35 μs | **1.6x** | **63%** |
| Batch 100 (Invalid) | 20 μs | 283 μs | **14x** | **92%** |

<details>
<summary>Full BenchmarkDotNet Results</summary>

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

**Key Findings:**
- **Largest gap on validation failures** - FluentValidation performance drops significantly on errors, CoreMesh stays stable
- **Huge memory allocation difference** - Invalid data: FluentValidation allocates 8.5KB vs CoreMesh 640B
- **Batch processing invalid data** - FluentValidation allocates 854KB vs CoreMesh 65KB

## Scope (Current Version)

Included:
- Fluent property rules
- Structured validation result
- Short-circuit validation (`StopOnInvalid`)
- DI registration extension (`AddValidatable`)
- Two usage modes (model implements or separate validator)

Not included yet:
- Async validation
- Conditional rules (`When/Unless`)
- Rule sets
- Collection rules (`RuleForEach`)
- Localization / error codes / severity levels
- ASP.NET Core automatic model validation integration
