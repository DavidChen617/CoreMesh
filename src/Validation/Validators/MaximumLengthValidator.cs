namespace CoreMesh.Validation.Validators;

public sealed class MaximumLengthValidator : IPropertyValidator<string?>
{
    private string? _message;

    private readonly int _max;

    public MaximumLengthValidator(int max)
    {
        if (max < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(max));
        }

        _max = max;
    }

    public bool IsValid(string? value)
    {
        if (value is null)
        {
            return false;
        }

        return value.Length <= _max;
    }

    public string GetErrorMessage(string propertyName)
    {
        return _message ?? $"'{propertyName}' length must be at most {_max}.";
    }


    public void SetMessage(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        _message = message;
    }
}
