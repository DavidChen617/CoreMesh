namespace CoreMesh.Validation.Validators;

public sealed class MinimumLengthValidator : IPropertyValidator<string?>
{
    private string? _message;

    private readonly int _min;

    public MinimumLengthValidator(int min)
    {
        if (min < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(min));
        }

        _min = min;
    }

    public bool IsValid(string? value)
    {
        if (value is null)
        {
            return false;
        }

        return value.Length >= _min;
    }

    public string GetErrorMessage(string propertyName)
    {
        return _message ?? $"'{propertyName}' length must be at least {_min}.";
    }


    public void SetMessage(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        _message = message;
    }
}
