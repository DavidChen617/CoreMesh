using CoreMesh.Validation.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace CoreMesh.Validation.Tests;

public sealed class ValidationTests
{
    private static IValidator CreateValidator()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IValidator, Validator>();
        var sp = services.BuildServiceProvider();
        return sp.GetRequiredService<IValidator>();
    }

    [Fact]
    public void Validate_Should_Return_Errors_When_Invalid()
    {
        var validator = CreateValidator();
        var command = new CreateUserCommand(null, "");

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 2);
        Assert.Contains(nameof(CreateUserCommand.Name), result.Errors.Keys);
        Assert.Contains(nameof(CreateUserCommand.Email), result.Errors.Keys);
    }

    [Fact]
    public void Validate_Should_Return_Valid_When_Input_Is_Valid()
    {
        var validator = CreateValidator();
        var command = new CreateUserCommand("David", "david@example.com");

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validator_Should_Cache_Rules_Per_Type()
    {
        CountingValidatable.ConfigureRulesCalls = 0;
        var validator = CreateValidator();

        _ = validator.Validate(new CountingValidatable(null));
        _ = validator.Validate(new CountingValidatable("ok"));
        _ = validator.Validate(new CountingValidatable("another"));

        Assert.Equal(1, CountingValidatable.ConfigureRulesCalls);
    }

    [Fact]
    public void Custom_Message_Should_Be_Used()
    {
        var validator = CreateValidator();

        var result = validator.Validate(new WithMessageCommand(""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors[nameof(WithMessageCommand.Name)], x => x == "Name cannot be blank.");
    }

    [Fact]
    public void Comparison_Rules_Should_Work_For_Numeric_Values()
    {
        var validator = CreateValidator();

        var invalid = validator.Validate(new ScoreCommand(5));
        Assert.False(invalid.IsValid);
        Assert.Contains(nameof(ScoreCommand.Score), invalid.Errors.Keys);

        var valid = validator.Validate(new ScoreCommand(50));
        Assert.True(valid.IsValid);
    }

    [Fact]
    public void Must_Rule_Should_Work()
    {
        var validator = CreateValidator();

        var invalid = validator.Validate(new MustRuleCommand(3));
        Assert.False(invalid.IsValid);

        var valid = validator.Validate(new MustRuleCommand(4));
        Assert.True(valid.IsValid);
    }

    [Fact]
    public void String_Length_Rules_Should_Work()
    {
        var validator = CreateValidator();

        var tooShort = validator.Validate(new StringLengthCommand("A"));
        Assert.False(tooShort.IsValid);

        var tooLong = validator.Validate(new StringLengthCommand("ABCDEFG"));
        Assert.False(tooLong.IsValid);

        var valid = validator.Validate(new StringLengthCommand("ABCD"));
        Assert.True(valid.IsValid);
    }

    [Fact]
    public void For_Should_Throw_For_Non_Member_Expression()
    {
        var validator = CreateValidator();

        var ex = Record.Exception(() => validator.Validate(new InvalidExpressionCommand("abc")));

        Assert.NotNull(ex);
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void StopOnInvalid_Should_Stop_Subsequent_Rules()
    {
        var validator = CreateValidator();

        var result = validator.Validate(new StopOnInvalidCommand(null));

        Assert.False(result.IsValid);
        // Should only have NotNull error, not MinLength error
        Assert.Single(result.Errors[nameof(StopOnInvalidCommand.Name)]);
    }

    // ===== Test Models =====

    public sealed record CreateUserCommand(string? Name, string? Email) : IValidatable<CreateUserCommand>
    {
        public void ConfigureValidateRules(ValidationBuilder<CreateUserCommand> builder)
        {
            builder.For(x => x.Name).NotNull().NotEmpty().MinLength(2).MaxLength(50);
            builder.For(x => x.Email).NotNull().NotEmpty();
        }
    }

    public sealed record CountingValidatable(string? Name) : IValidatable<CountingValidatable>
    {
        public static int ConfigureRulesCalls { get; set; }

        public void ConfigureValidateRules(ValidationBuilder<CountingValidatable> builder)
        {
            ConfigureRulesCalls++;
            builder.For(x => x.Name).NotNull();
        }
    }

    public sealed record WithMessageCommand(string? Name) : IValidatable<WithMessageCommand>
    {
        public void ConfigureValidateRules(ValidationBuilder<WithMessageCommand> builder)
        {
            builder.For(x => x.Name).NotEmpty("Name cannot be blank.");
        }
    }

    public sealed record ScoreCommand(int Score) : IValidatable<ScoreCommand>
    {
        public void ConfigureValidateRules(ValidationBuilder<ScoreCommand> builder)
        {
            builder.For(x => x.Score)
                .GreaterThan(10)
                .LessThan(100);
        }
    }

    public sealed record MustRuleCommand(int Quantity) : IValidatable<MustRuleCommand>
    {
        public void ConfigureValidateRules(ValidationBuilder<MustRuleCommand> builder)
        {
            builder.For(x => x.Quantity).Must(x => x % 2 == 0, "Must be even.");
        }
    }

    public sealed record StringLengthCommand(string? Name) : IValidatable<StringLengthCommand>
    {
        public void ConfigureValidateRules(ValidationBuilder<StringLengthCommand> builder)
        {
            builder.For(x => x.Name)
                .NotNull()
                .NotEmpty()
                .MinLength(2)
                .MaxLength(5);
        }
    }

    public sealed record InvalidExpressionCommand(string? Name) : IValidatable<InvalidExpressionCommand>
    {
        public void ConfigureValidateRules(ValidationBuilder<InvalidExpressionCommand> builder)
        {
            builder.For(x => x.Name + "x").NotEmpty();
        }
    }

    public sealed record StopOnInvalidCommand(string? Name) : IValidatable<StopOnInvalidCommand>
    {
        public void ConfigureValidateRules(ValidationBuilder<StopOnInvalidCommand> builder)
        {
            builder.For(x => x.Name)
                .NotNull()
                .StopOnInvalid()
                .MinLength(5);
        }
    }
}
