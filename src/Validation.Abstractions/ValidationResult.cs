namespace CoreMesh.Validation.Abstractions;

/// <summary>
/// Represents the result of a validation operation.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Gets a value indicating whether the validation passed (no errors).
    /// </summary>
    public bool IsValid => Errors.Count == 0;

    /// <summary>
    /// Gets the dictionary of validation errors, where the key is the property name
    /// and the value is a list of error messages for that property.
    /// </summary>
    public Dictionary<string, IReadOnlyList<string>> Errors { get; } = new();
}
