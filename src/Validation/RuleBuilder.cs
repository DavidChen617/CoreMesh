using CoreMesh.Validation.Validators;

namespace CoreMesh.Validation;

public sealed class RuleBuilder<T, TProperty>(PropertyRule<T, TProperty> rule)
{
    private int? _lastComponentIndex;

    public RuleBuilder<T, TProperty> SetValidator(IPropertyValidator<TProperty> validator)
    {
        _lastComponentIndex = rule.AddValidator(validator);
        return this;
    }

    public RuleBuilder<T, TProperty> WithMessage(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        if (_lastComponentIndex is null)
        {
            throw new InvalidOperationException("WithMessage() must be called after a validator method.");
        }

        rule.SetCustomMessage(_lastComponentIndex.Value, message);
        return this;
    }

    public RuleBuilder<T, TProperty> Must(Func<TProperty, bool> predicate, string message)
    {
        return SetValidator(new PredicateValidator<TProperty>(predicate, message));
    }

    public RuleBuilder<T, TProperty> Equal(TProperty expected)
    {
        return SetValidator(new EqualValidator<TProperty>(expected));
    }

    public RuleBuilder<T, TProperty> NotNull()
    {
        return SetValidator(new NotNullValidator<TProperty>());
    }

    public RuleBuilder<T, TProperty> NotEmpty()
    {
        EnsureString(nameof(NotEmpty));
        return SetValidator((IPropertyValidator<TProperty>)(object)new NotEmptyStringValidator());
    }

    public RuleBuilder<T, TProperty> Length(int min, int max)
    {
        EnsureString(nameof(Length));
        return SetValidator((IPropertyValidator<TProperty>)(object)new StringLengthValidator(min, max));
    }

    public RuleBuilder<T, TProperty> MinimumLength(int min)
    {
        EnsureString(nameof(MinimumLength));
        return SetValidator((IPropertyValidator<TProperty>)(object)new MinimumLengthValidator(min));
    }

    public RuleBuilder<T, TProperty> MaximumLength(int max)
    {
        EnsureString(nameof(MaximumLength));
        return SetValidator((IPropertyValidator<TProperty>)(object)new MaximumLengthValidator(max));
    }

    public RuleBuilder<T, TProperty> Regex(string pattern)
    {
        EnsureString(nameof(Regex));
        return SetValidator((IPropertyValidator<TProperty>)(object)new RegexValidator(pattern));
    }

    public RuleBuilder<T, TProperty> Range(TProperty min, TProperty max)
    {
        return SetValidator(new RangeValidator<TProperty>(min, max));
    }

    public RuleBuilder<T, TProperty> GreaterThan(TProperty value)
    {
        return SetValidator(new GreaterThanValidator<TProperty>(value));
    }

    public RuleBuilder<T, TProperty> LessThan(TProperty value)
    {
        return SetValidator(new LessThanValidator<TProperty>(value));
    }

    private static void EnsureString(string methodName)
    {
        if (typeof(TProperty) != typeof(string))
        {
            throw new NotSupportedException($"{methodName}() currently supports string properties only in MVP.");
        }
    }
}
