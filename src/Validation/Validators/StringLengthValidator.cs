namespace CoreMesh.Validation.Validators;

public sealed class StringLengthValidator : IPropertyValidator<string?>
{
    private string? _message;

    private readonly int _min;
    private readonly int _max;

    public StringLengthValidator(int min, int max)
    {
        if (min < 0)
            throw new ArgumentOutOfRangeException(nameof(min));

        if (max < min)
            throw new ArgumentOutOfRangeException(nameof(max));

        _min = min;
        _max = max;
    }

    public bool IsValid(string? value)
    {
        if (value is null)
        {
            return false;
        }

        return value.Length >= _min && value.Length <= _max;
    }

    public string GetErrorMessage(string propertyName)
    {
        return _message ?? $"'{propertyName}' length must be between {_min} and {_max}.";
    }

    public void SetMessage(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        _message = message;
    }
}
