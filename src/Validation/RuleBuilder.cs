using CoreMesh.Validation.Validators;

namespace CoreMesh.Validation;

/// <summary>
/// Provides a fluent API for configuring validators on a single property rule.
/// </summary>
/// <typeparam name="T">The model type.</typeparam>
/// <typeparam name="TProperty">The property type.</typeparam>
public sealed class RuleBuilder<T, TProperty>(PropertyRule<T, TProperty> rule)
{
    private int? _lastComponentIndex;

    /// <summary>
    /// Adds a custom property validator.
    /// </summary>
    /// <param name="validator">The validator to add.</param>
    /// <returns>The current rule builder.</returns>
    public RuleBuilder<T, TProperty> SetValidator(IPropertyValidator<TProperty> validator)
    {
        _lastComponentIndex = rule.AddValidator(validator);
        return this;
    }

    /// <summary>
    /// Overrides the error message for the previously added validator in the chain.
    /// </summary>
    /// <param name="message">The custom error message.</param>
    /// <returns>The current rule builder.</returns>
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

    /// <summary>
    /// Adds a predicate-based validator.
    /// </summary>
    public RuleBuilder<T, TProperty> Must(Func<TProperty, bool> predicate, string message)
    {
        return SetValidator(new PredicateValidator<TProperty>(predicate, message));
    }

    /// <summary>
    /// Adds an equality validator.
    /// </summary>
    public RuleBuilder<T, TProperty> Equal(TProperty expected)
    {
        return SetValidator(new EqualValidator<TProperty>(expected));
    }

    /// <summary>
    /// Adds a non-null validator.
    /// </summary>
    public RuleBuilder<T, TProperty> NotNull()
    {
        return SetValidator(new NotNullValidator<TProperty>());
    }

    /// <summary>
    /// Adds a non-empty string validator.
    /// </summary>
    public RuleBuilder<T, TProperty> NotEmpty()
    {
        EnsureString(nameof(NotEmpty));
        return SetValidator((IPropertyValidator<TProperty>)(object)new NotEmptyStringValidator());
    }

    /// <summary>
    /// Adds a string length range validator.
    /// </summary>
    public RuleBuilder<T, TProperty> Length(int min, int max)
    {
        EnsureString(nameof(Length));
        return SetValidator((IPropertyValidator<TProperty>)(object)new StringLengthValidator(min, max));
    }

    /// <summary>
    /// Adds a minimum string length validator.
    /// </summary>
    public RuleBuilder<T, TProperty> MinimumLength(int min)
    {
        EnsureString(nameof(MinimumLength));
        return SetValidator((IPropertyValidator<TProperty>)(object)new MinimumLengthValidator(min));
    }

    /// <summary>
    /// Adds a maximum string length validator.
    /// </summary>
    public RuleBuilder<T, TProperty> MaximumLength(int max)
    {
        EnsureString(nameof(MaximumLength));
        return SetValidator((IPropertyValidator<TProperty>)(object)new MaximumLengthValidator(max));
    }

    /// <summary>
    /// Adds a regex validator for string values.
    /// </summary>
    public RuleBuilder<T, TProperty> Regex(string pattern)
    {
        EnsureString(nameof(Regex));
        return SetValidator((IPropertyValidator<TProperty>)(object)new RegexValidator(pattern));
    }

    /// <summary>
    /// Adds a range validator.
    /// </summary>
    public RuleBuilder<T, TProperty> Range(TProperty min, TProperty max)
    {
        return SetValidator(new RangeValidator<TProperty>(min, max));
    }

    /// <summary>
    /// Adds a greater-than validator.
    /// </summary>
    public RuleBuilder<T, TProperty> GreaterThan(TProperty value)
    {
        return SetValidator(new GreaterThanValidator<TProperty>(value));
    }

    /// <summary>
    /// Adds a less-than validator.
    /// </summary>
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
