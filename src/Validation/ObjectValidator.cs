namespace CoreMesh.Validation;

public sealed class ObjectValidator<T>
{
    private readonly IReadOnlyList<IValidationRule<T>> _rules;

    public ObjectValidator(IReadOnlyList<IValidationRule<T>> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);
        _rules = rules;
    }

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
