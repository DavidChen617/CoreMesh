namespace CoreMesh.Validation.Validators;

/// <summary>
/// Provides property validation logic for this rule type.
/// </summary>
public sealed class LessThanValidator<TProperty> : IPropertyValidator<TProperty>
{

    private readonly TProperty _value;
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>

    public LessThanValidator(TProperty value)
    {
        _value = value;
    }
    /// <summary>
    /// Determines whether the value is valid.
    /// </summary>

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
    /// <summary>
    /// Gets the error message for a failed validation.
    /// </summary>

    public string GetErrorMessage(string propertyName)
    {
        return $"'{propertyName}' must be less than '{_value}'.";
    }
}
