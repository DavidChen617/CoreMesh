namespace CoreMesh.Validation.Tests;

public sealed class ValidationTests
{
    [Fact]
    public void Validate_Should_Return_Errors_When_Invalid()
    {
        var validator = new Validator<CreateUserCommand>();
        var command = new CreateUserCommand(null, "");

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Equal(4, result.Errors.Count);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CreateUserCommand.Name));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(CreateUserCommand.Email));
    }

    [Fact]
    public void Validate_Should_Return_Valid_When_Input_Is_Valid()
    {
        var validator = new Validator<CreateUserCommand>();
        var command = new CreateUserCommand("David", "david@example.com");

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateAndThrow_Should_Throw_When_Invalid()
    {
        var validator = new Validator<CreateUserCommand>();
        var command = new CreateUserCommand(null, "");

        var ex = Record.Exception(() => validator.ValidateAndThrow(command));
        Assert.NotNull(ex);
        Assert.Equal("ValidationException", ex!.GetType().Name);
    }

    [Fact]
    public void ValidateAndThrow_Should_Use_Custom_Message()
    {
        var validator = new Validator<CreateUserCommand>();
        var command = new CreateUserCommand(null, "");

        var ex = Record.Exception(() => validator.ValidateAndThrow(command, "Command invalid."));
        Assert.NotNull(ex);
        Assert.Equal("Command invalid.", ex!.Message);
    }

    [Fact]
    public void Validator_Should_Cache_ObjectValidator_Per_Type()
    {
        CountingValidatable.ConfigureRulesCalls = 0;
        var validator = new Validator<CountingValidatable>();

        _ = validator.Validate(new CountingValidatable(null));
        _ = validator.Validate(new CountingValidatable("ok"));
        _ = validator.Validate(new CountingValidatable("another"));

        Assert.Equal(1, CountingValidatable.ConfigureRulesCalls);
    }

    [Fact]
    public void WithMessage_Should_Override_Last_Validator_Message()
    {
        var validator = new Validator<WithMessageCommand>();

        var result = validator.Validate(new WithMessageCommand(""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "Name cannot be blank.");
    }

    [Fact]
    public void Regex_And_Length_Rules_Should_Work_For_String()
    {
        var validator = new Validator<RegexCommand>();

        var invalid = validator.Validate(new RegexCommand("ab-1"));
        Assert.False(invalid.IsValid);
        Assert.True(invalid.Errors.Count >= 1);

        var valid = validator.Validate(new RegexCommand("ABC12"));
        Assert.True(valid.IsValid);
    }

    [Fact]
    public void Comparison_Rules_Should_Work_For_Numeric_Values()
    {
        var validator = new Validator<ScoreCommand>();

        var invalid = validator.Validate(new ScoreCommand(5));
        Assert.False(invalid.IsValid);
        Assert.Contains(invalid.Errors, x => x.PropertyName == nameof(ScoreCommand.Score));

        var valid = validator.Validate(new ScoreCommand(50));
        Assert.True(valid.IsValid);
    }

    [Fact]
    public void Must_And_Equal_Rules_Should_Work()
    {
        var validator = new Validator<MixedRuleCommand>();

        var invalid = validator.Validate(new MixedRuleCommand("wrong", 3));
        Assert.False(invalid.IsValid);
        Assert.Equal(2, invalid.Errors.Count);

        var valid = validator.Validate(new MixedRuleCommand("OK", 4));
        Assert.True(valid.IsValid);
    }


    [Fact]
    public void WithMessage_Should_Only_Override_Previous_Validator()
    {
        var validator = new Validator<WithMessageOrderCommand>();

        var result = validator.Validate(new WithMessageOrderCommand(""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage == "Name is required.");
        Assert.Contains(result.Errors, x => x.ErrorMessage.Contains("length must be at least 2", StringComparison.OrdinalIgnoreCase));
    }


    [Fact]
    public void String_Length_Rules_Should_Work_Explicitly()
    {
        var validator = new Validator<StringLengthCommand>();

        var invalid = validator.Validate(new StringLengthCommand("A"));
        Assert.False(invalid.IsValid);
        Assert.Contains(invalid.Errors, x => x.ErrorMessage.Contains("at least 2", StringComparison.OrdinalIgnoreCase));

        var invalidTooLong = validator.Validate(new StringLengthCommand("ABCDEFG"));
        Assert.False(invalidTooLong.IsValid);
        Assert.Contains(invalidTooLong.Errors, x => x.ErrorMessage.Contains("at most 5", StringComparison.OrdinalIgnoreCase));

        var valid = validator.Validate(new StringLengthCommand("ABCD"));
        Assert.True(valid.IsValid);
    }

    [Fact]
    public void WithMessage_Should_Throw_When_No_Previous_Validator()
    {
        var validator = new Validator<WithMessageWithoutValidatorCommand>();

        var ex = Record.Exception(() => validator.Validate(new WithMessageWithoutValidatorCommand("x")));

        Assert.NotNull(ex);
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public void String_Only_Rules_Should_Throw_For_Non_String_Property()
    {
        var validator = new Validator<NonStringLengthCommand>();

        var ex = Record.Exception(() => validator.Validate(new NonStringLengthCommand(123)));

        Assert.NotNull(ex);
        Assert.IsType<NotSupportedException>(ex);
    }

    [Fact]
    public void Comparison_Rules_Should_Throw_For_Non_Comparable_Type()
    {
        var validator = new Validator<NonComparableCompareCommand>();

        var ex = Record.Exception(() => validator.Validate(new NonComparableCompareCommand(new NonComparableValue(2))));

        Assert.NotNull(ex);
        Assert.IsType<NotSupportedException>(ex);
    }

    [Fact]
    public void RuleFor_Should_Throw_For_Non_Member_Expression()
    {
        var validator = new Validator<InvalidExpressionCommand>();

        var ex = Record.Exception(() => validator.Validate(new InvalidExpressionCommand("abc")));

        Assert.NotNull(ex);
        Assert.IsType<InvalidOperationException>(ex);
    }

    public sealed record CreateUserCommand(string? Name, string? Email) : IValidatable<CreateUserCommand>
    {
        public void ConfigureRules(ValidationBuilder<CreateUserCommand> builder)
        {
            builder.RuleFor(x => x.Name).NotNull().NotEmpty().Length(2, 50);
            builder.RuleFor(x => x.Email).NotNull().NotEmpty();
        }
    }

    public sealed record CountingValidatable(string? Name) : IValidatable<CountingValidatable>
    {
        public static int ConfigureRulesCalls { get; set; }

        public void ConfigureRules(ValidationBuilder<CountingValidatable> builder)
        {
            ConfigureRulesCalls++;
            builder.RuleFor(x => x.Name).NotNull();
        }
    }

    public sealed record WithMessageCommand(string? Name) : IValidatable<WithMessageCommand>
    {
        public void ConfigureRules(ValidationBuilder<WithMessageCommand> builder)
        {
            builder.RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name cannot be blank.");
        }
    }

    public sealed record RegexCommand(string? Code) : IValidatable<RegexCommand>
    {
        public void ConfigureRules(ValidationBuilder<RegexCommand> builder)
        {
            builder.RuleFor(x => x.Code)
                .NotEmpty()
                .Length(5, 5)
                .Regex("^[A-Z0-9]+$");
        }
    }

    public sealed record ScoreCommand(int Score) : IValidatable<ScoreCommand>
    {
        public void ConfigureRules(ValidationBuilder<ScoreCommand> builder)
        {
            builder.RuleFor(x => x.Score)
                .GreaterThan(10)
                .LessThan(100)
                .Range(20, 80);
        }
    }

    public sealed record MixedRuleCommand(string? Status, int Quantity) : IValidatable<MixedRuleCommand>
    {
        public void ConfigureRules(ValidationBuilder<MixedRuleCommand> builder)
        {
            builder.RuleFor(x => x.Status).Equal("OK");
            builder.RuleFor(x => x.Quantity).Must(x => x % 2 == 0, "'{PropertyName}' must be even.");
        }
    }


    public sealed record WithMessageOrderCommand(string? Name) : IValidatable<WithMessageOrderCommand>
    {
        public void ConfigureRules(ValidationBuilder<WithMessageOrderCommand> builder)
        {
            builder.RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MinimumLength(2);
        }
    }

    public sealed record StringLengthCommand(string? Name) : IValidatable<StringLengthCommand>
    {
        public void ConfigureRules(ValidationBuilder<StringLengthCommand> builder)
        {
            builder.RuleFor(x => x.Name)
                .NotNull()
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(5);
        }
    }

    public sealed record WithMessageWithoutValidatorCommand(string? Name) : IValidatable<WithMessageWithoutValidatorCommand>
    {
        public void ConfigureRules(ValidationBuilder<WithMessageWithoutValidatorCommand> builder)
        {
            builder.RuleFor(x => x.Name).WithMessage("Should fail");
        }
    }

    public sealed record NonStringLengthCommand(int Value) : IValidatable<NonStringLengthCommand>
    {
        public void ConfigureRules(ValidationBuilder<NonStringLengthCommand> builder)
        {
            builder.RuleFor(x => x.Value).Length(1, 3);
        }
    }

    public sealed record NonComparableValue(int Value);

    public sealed record NonComparableCompareCommand(NonComparableValue Value) : IValidatable<NonComparableCompareCommand>
    {
        public void ConfigureRules(ValidationBuilder<NonComparableCompareCommand> builder)
        {
            builder.RuleFor(x => x.Value).GreaterThan(new NonComparableValue(1));
        }
    }

    public sealed record InvalidExpressionCommand(string? Name) : IValidatable<InvalidExpressionCommand>
    {
        public void ConfigureRules(ValidationBuilder<InvalidExpressionCommand> builder)
        {
            builder.RuleFor(x => x.Name + "x").NotEmpty();
        }
    }
}
