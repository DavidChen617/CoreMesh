namespace CoreMesh.Validation.Validators;

/// <summary>
/// Provides property validation logic for this rule type.
/// </summary>
public sealed class MinimumLengthValidator : IPropertyValidator<string?>
{

    private readonly int _min;
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>

    public MinimumLengthValidator(int min)
    {
        if (min < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(min));
        }

        _min = min;
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

        return value.Length >= _min;
    }
    /// <summary>
    /// Gets the error message for a failed validation.
    /// </summary>

    public string GetErrorMessage(string propertyName)
    {
        return $"'{propertyName}' length must be at least {_min}.";
    }
}
