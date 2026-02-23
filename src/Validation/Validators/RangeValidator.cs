namespace CoreMesh.Validation.Validators;

/// <summary>
/// Provides property validation logic for this rule type.
/// </summary>
public sealed class RangeValidator<TProperty> : IPropertyValidator<TProperty>
{

    private readonly TProperty _min;
    private readonly TProperty _max;
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>

    public RangeValidator(TProperty min, TProperty max)
    {
        _min = min;
        _max = max;
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

        return comparable.CompareTo(_min) >= 0 && comparable.CompareTo(_max) <= 0;
    }
    /// <summary>
    /// Gets the error message for a failed validation.
    /// </summary>

    public string GetErrorMessage(string propertyName)
    {
        return $"'{propertyName}' must be between '{_min}' and '{_max}'.";
    }
}
