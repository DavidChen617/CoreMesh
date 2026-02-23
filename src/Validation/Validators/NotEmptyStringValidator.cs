namespace CoreMesh.Validation.Validators;

/// <summary>
/// Provides property validation logic for this rule type.
/// </summary>
public sealed class NotEmptyStringValidator : IPropertyValidator<string?>
{
    /// <summary>
    /// Determines whether the value is valid.
    /// </summary>

    public bool IsValid(string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }
    /// <summary>
    /// Gets the error message for a failed validation.
    /// </summary>

    public string GetErrorMessage(string propertyName)
    {
        return $"'{propertyName}' must not be empty.";
    }
}
