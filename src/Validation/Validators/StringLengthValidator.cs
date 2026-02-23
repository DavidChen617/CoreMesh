namespace CoreMesh.Validation.Validators;

/// <summary>
/// Provides property validation logic for this rule type.
/// </summary>
public sealed class StringLengthValidator : IPropertyValidator<string?>
{

    private readonly int _min;
    private readonly int _max;
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>

    public StringLengthValidator(int min, int max)
    {
        if (min < 0)
            throw new ArgumentOutOfRangeException(nameof(min));

        if (max < min)
            throw new ArgumentOutOfRangeException(nameof(max));

        _min = min;
        _max = max;
    }
    /// <summary>
    /// Determines whether the value is valid.
    /// </summary>

    public bool IsValid(string? value)
    {
        if (value is null)
        {
            return false;
        }

        return value.Length >= _min && value.Length <= _max;
    }
    /// <summary>
    /// Gets the error message for a failed validation.
    /// </summary>

    public string GetErrorMessage(string propertyName)
    {
        return $"'{propertyName}' length must be between {_min} and {_max}.";
    }
}
