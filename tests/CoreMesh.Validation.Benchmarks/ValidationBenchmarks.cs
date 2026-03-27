using BenchmarkDotNet.Attributes;
using CoreMesh.Validation.Abstractions;
using CoreMesh.Validation.Abstractions.Extensions;
using CoreMesh.Validation.Extensions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using CoreMeshValidator = CoreMesh.Validation.Abstractions.IValidator;

namespace CoreMesh.Validation.Benchmarks;

[MemoryDiagnoser]
public class ValidationBenchmarks
{
    private CoreMeshValidator _coreMeshValidator = default!;
    private IValidator<CreateUserCommand> _fluentValidator = default!;

    private CreateUserCommand _validCommand = default!;
    private CreateUserCommand _invalidCommand = default!;
    private List<CreateUserCommand> _validCommands = default!;
    private List<CreateUserCommand> _invalidCommands = default!;

    [GlobalSetup]
    public void Setup()
    {
        // CoreMesh.Validation setup
        var services = new ServiceCollection();
        services.AddSingleton<CoreMeshValidator, Validator>();
        var sp = services.BuildServiceProvider();
        _coreMeshValidator = sp.GetRequiredService<CoreMeshValidator>();

        // FluentValidation setup
        _fluentValidator = new CreateUserCommandFluentValidator();

        // Test data
        _validCommand = new CreateUserCommand("John Doe", "john@example.com", 25);
        _invalidCommand = new CreateUserCommand("", "", -1);

        _validCommands = Enumerable.Range(0, 100)
            .Select(i => new CreateUserCommand($"User{i}", $"user{i}@example.com", 20 + i))
            .ToList();

        _invalidCommands = Enumerable.Range(0, 100)
            .Select(i => new CreateUserCommand("", "", -i))
            .ToList();
    }

    // ===== Single Valid =====

    [Benchmark]
    public ValidationResult CoreMesh_Validate_Valid()
        => _coreMeshValidator.Validate(_validCommand);

    [Benchmark]
    public FluentValidation.Results.ValidationResult FluentValidation_Validate_Valid()
        => _fluentValidator.Validate(_validCommand);

    // ===== Single Invalid =====

    [Benchmark]
    public ValidationResult CoreMesh_Validate_Invalid()
        => _coreMeshValidator.Validate(_invalidCommand);

    [Benchmark]
    public FluentValidation.Results.ValidationResult FluentValidation_Validate_Invalid()
        => _fluentValidator.Validate(_invalidCommand);

    // ===== Collection Valid (100 items) =====

    [Benchmark]
    public List<ValidationResult> CoreMesh_Validate_Collection_Valid_100()
        => _validCommands.Select(c => _coreMeshValidator.Validate(c)).ToList();

    [Benchmark]
    public List<FluentValidation.Results.ValidationResult> FluentValidation_Validate_Collection_Valid_100()
        => _validCommands.Select(c => _fluentValidator.Validate(c)).ToList();

    // ===== Collection Invalid (100 items) =====

    [Benchmark]
    public List<ValidationResult> CoreMesh_Validate_Collection_Invalid_100()
        => _invalidCommands.Select(c => _coreMeshValidator.Validate(c)).ToList();

    [Benchmark]
    public List<FluentValidation.Results.ValidationResult> FluentValidation_Validate_Collection_Invalid_100()
        => _invalidCommands.Select(c => _fluentValidator.Validate(c)).ToList();

    // ===== Test Models =====

    public sealed record CreateUserCommand(string? Name, string? Email, int Age)
        : IValidatable<CreateUserCommand>
    {
        public void ConfigureValidateRules(IValidationBuilder<CreateUserCommand> builder)
        {
            builder.For(x => x.Name)
                .NotNull()
                .NotEmpty()
                .MinLength(2)
                .MaxLength(50);

            builder.For(x => x.Email)
                .NotNull()
                .NotEmpty()
                .EmailAddress();

            builder.For(x => x.Age)
                .GreaterThanOrEqual(0)
                .LessThanOrEqual(150);
        }
    }

    // FluentValidation validator
    public sealed class CreateUserCommandFluentValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandFluentValidator()
        {
            RuleFor(x => x.Name)
                .NotNull()
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(50);

            RuleFor(x => x.Email)
                .NotNull()
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Age)
                .GreaterThanOrEqualTo(0)
                .LessThanOrEqualTo(150);
        }
    }
}
