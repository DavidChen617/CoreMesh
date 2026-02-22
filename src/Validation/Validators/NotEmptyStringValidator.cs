namespace CoreMesh.Validation.Validators;

public sealed class NotEmptyStringValidator : IPropertyValidator<string?>
{
    private string? _message;

    public bool IsValid(string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    public string GetErrorMessage(string propertyName)
    {
        return _message ?? $"'{propertyName}' must not be empty.";
    }


    public void SetMessage(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        _message = message;
    }
}
