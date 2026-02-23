namespace CoreMesh.Validation.Validators;

/// <summary>
/// Provides property validation logic for this rule type.
/// </summary>
public sealed class EqualValidator<TProperty> : IPropertyValidator<TProperty>
{

    private readonly TProperty _expected;
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>

    public EqualValidator(TProperty expected)
    {
        _expected = expected;
    }
    /// <summary>
    /// Determines whether the value is valid.
    /// </summary>

    public bool IsValid(TProperty value)
    {
        return EqualityComparer<TProperty>.Default.Equals(value, _expected);
    }
    /// <summary>
    /// Gets the error message for a failed validation.
    /// </summary>

    public string GetErrorMessage(string propertyName)
    {
        return $"'{propertyName}' must be equal to '{_expected}'.";
    }
}
