namespace CoreMesh.Validation.Validators;

public sealed class RangeValidator<TProperty> : IPropertyValidator<TProperty>
{
    private string? _message;

    private readonly TProperty _min;
    private readonly TProperty _max;

    public RangeValidator(TProperty min, TProperty max)
    {
        _min = min;
        _max = max;
    }

    public bool IsValid(TProperty value)
    {
        if (value is null)
        {
            return false;
        }

        if (value is not IComparable<TProperty> comparable)
        {
            throw new NotSupportedException($"Type '{typeof(TProperty).Name}' does not support comparison.");
        }

        return comparable.CompareTo(_min) >= 0 && comparable.CompareTo(_max) <= 0;
    }

    public string GetErrorMessage(string propertyName)
    {
        return _message ?? $"'{propertyName}' must be between '{_min}' and '{_max}'.";
    }


    public void SetMessage(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        _message = message;
    }
}
