namespace CoreMesh.Validation;

public sealed class PropertyRule<T, TProperty>(string propertyName, Func<T, TProperty> propertyFunc) : IValidationRule<T>
{
    private readonly List<PropertyRuleComponent> _components = [];

    public int AddValidator(IPropertyValidator<TProperty> validator)
    {
        ArgumentNullException.ThrowIfNull(validator);

        _components.Add(new PropertyRuleComponent(validator));
        return _components.Count - 1;
    }

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
