namespace CoreMesh.Validation;

/// <summary>
/// Executes validation rules for a model type.
/// </summary>
/// <typeparam name="T">The model type.</typeparam>
public sealed class ObjectValidator<T>
{
    private readonly IReadOnlyList<IValidationRule<T>> _rules;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectValidator{T}"/> class.
    /// </summary>
    /// <param name="rules">The rules to execute.</param>
    public ObjectValidator(IReadOnlyList<IValidationRule<T>> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);
        _rules = rules;
    }

    /// <summary>
    /// Validates the specified instance.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <returns>The validation result.</returns>
    public ValidationResult Validate(T instance)
    {
        var failures = new List<ValidationFailure>();

        foreach (var rule in _rules)
        {
            rule.Validate(instance, failures);
        }

        return new ValidationResult(failures);
    }
}
