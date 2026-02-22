namespace CoreMesh.Validation.Validators;

public sealed class LessThanValidator<TProperty> : IPropertyValidator<TProperty>
{
    private string? _message;

    private readonly TProperty _value;

    public LessThanValidator(TProperty value)
    {
        _value = value;
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

        return comparable.CompareTo(_value) < 0;
    }

    public string GetErrorMessage(string propertyName)
    {
        return _message ?? $"'{propertyName}' must be less than '{_value}'.";
    }


    public void SetMessage(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        _message = message;
    }
}
