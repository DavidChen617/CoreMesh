namespace CoreMesh.Validation.Validators;

public sealed class NotNullValidator<TProperty> : IPropertyValidator<TProperty>
{
    private string? _message;

    public bool IsValid(TProperty value)
    {
        return value is not null;
    }

    public string GetErrorMessage(string propertyName)
    {
        return _message ?? $"'{propertyName}' must not be null.";
    }


    public void SetMessage(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        _message = message;
    }
}
