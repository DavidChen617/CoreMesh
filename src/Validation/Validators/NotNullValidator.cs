namespace CoreMesh.Validation.Validators;

/// <summary>
/// Provides property validation logic for this rule type.
/// </summary>
public sealed class NotNullValidator<TProperty> : IPropertyValidator<TProperty>
{
    /// <summary>
    /// Determines whether the value is valid.
    /// </summary>

    public bool IsValid(TProperty value)
    {
        return value is not null;
    }
    /// <summary>
    /// Gets the error message for a failed validation.
    /// </summary>

    public string GetErrorMessage(string propertyName)
    {
        return $"'{propertyName}' must not be null.";
    }
}
