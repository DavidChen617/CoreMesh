namespace CoreMesh.Validation.Validators;

public sealed class PredicateValidator<TProperty> : IPropertyValidator<TProperty>
{
    private readonly Func<TProperty, bool> _predicate;
    private readonly string _messageTemplate;
    private string? _message;

    public PredicateValidator(Func<TProperty, bool> predicate, string message)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        _predicate = predicate;
        _messageTemplate = message;
    }

    public bool IsValid(TProperty value) => _predicate(value);

    public string GetErrorMessage(string propertyName)
        => (_message ?? _messageTemplate).Replace("{PropertyName}", propertyName);

    public void SetMessage(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        _message = message;
    }
}
