namespace CoreMesh.Validation.Validators;

/// <summary>
/// Provides property validation logic for this rule type.
/// </summary>
public sealed class MaximumLengthValidator : IPropertyValidator<string?>
{

    private readonly int _max;
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>

    public MaximumLengthValidator(int max)
    {
        if (max < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(max));
        }

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

        return value.Length <= _max;
    }
    /// <summary>
    /// Gets the error message for a failed validation.
    /// </summary>

    public string GetErrorMessage(string propertyName)
    {
        return $"'{propertyName}' length must be at most {_max}.";
    }
}
