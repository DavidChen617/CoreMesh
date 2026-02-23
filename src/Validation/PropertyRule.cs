namespace CoreMesh.Validation;

/// <summary>
/// Represents a validation rule for a single property.
/// </summary>
/// <typeparam name="T">The model type.</typeparam>
/// <typeparam name="TProperty">The property type.</typeparam>
public sealed class PropertyRule<T, TProperty>(string propertyName, Func<T, TProperty> propertyFunc) : IValidationRule<T>
{
    private readonly List<PropertyRuleComponent> _components = [];

    /// <summary>
    /// Adds a property validator to this rule.
    /// </summary>
    /// <param name="validator">The property validator.</param>
    /// <returns>The component index for later metadata updates.</returns>
    public int AddValidator(IPropertyValidator<TProperty> validator)
    {
        ArgumentNullException.ThrowIfNull(validator);

        _components.Add(new PropertyRuleComponent(validator));
        return _components.Count - 1;
    }

    /// <summary>
    /// Sets a custom error message for a previously added validator component.
    /// </summary>
    /// <param name="componentIndex">The validator component index.</param>
    /// <param name="message">The custom error message.</param>
    public void SetCustomMessage(int componentIndex, string message)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(componentIndex);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        if (componentIndex >= _components.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(componentIndex));
        }

        _components[componentIndex].CustomMessage = message;
    }

    /// <summary>
    /// Validates the property value and appends failures to the result list.
    /// </summary>
    /// <param name="instance">The model instance.</param>
    /// <param name="failures">The failure collection.</param>
    public void Validate(T instance, List<ValidationFailure> failures)
    {
        ArgumentNullException.ThrowIfNull(failures);
        var value = propertyFunc(instance);

        foreach (var component in _components)
        {
            if (component.Validator.IsValid(value))
            {
                continue;
            }

            var errorMessage = component.CustomMessage ?? component.Validator.GetErrorMessage(propertyName);

            failures.Add(new ValidationFailure(
                propertyName,
                errorMessage,
                value));
        }
    }

    private sealed class PropertyRuleComponent(IPropertyValidator<TProperty> validator)
    {
        public IPropertyValidator<TProperty> Validator { get; } = validator;

        public string? CustomMessage { get; set; }
    }
}
