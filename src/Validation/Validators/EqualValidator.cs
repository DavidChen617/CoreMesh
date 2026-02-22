namespace CoreMesh.Validation.Validators;

public sealed class EqualValidator<TProperty> : IPropertyValidator<TProperty>
{
    private string? _message;

    private readonly TProperty _expected;

    public EqualValidator(TProperty expected)
    {
        _expected = expected;
    }

    public bool IsValid(TProperty value)
    {
        return EqualityComparer<TProperty>.Default.Equals(value, _expected);
    }

    public string GetErrorMessage(string propertyName)
    {
        return _message ?? $"'{propertyName}' must be equal to '{_expected}'.";
    }


    public void SetMessage(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        _message = message;
    }
}
