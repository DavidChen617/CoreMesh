namespace CoreMesh.Validation.Validators;

/// <summary>
/// Provides property validation logic for this rule type.
/// </summary>
public sealed class PredicateValidator<TProperty> : IPropertyValidator<TProperty>
{
    private readonly Func<TProperty, bool> _predicate;
    private readonly string _messageTemplate;
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>

    public PredicateValidator(Func<TProperty, bool> predicate, string message)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        _predicate = predicate;
        _messageTemplate = message;
    }
    /// <summary>
    /// Determines whether the value is valid.
    /// </summary>

    public bool IsValid(TProperty value) => _predicate(value);
    /// <summary>
    /// Gets the error message for a failed validation.
    /// </summary>

    public string GetErrorMessage(string propertyName)
        => _messageTemplate.Replace("{PropertyName}", propertyName);
}
