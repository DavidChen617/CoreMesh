namespace CoreMesh.Validation;

/// <summary>
/// Provides validation operations for a specific validatable model type.
/// </summary>
/// <typeparam name="T">The model type.</typeparam>
public interface IValidator<T> where T : IValidatable<T>
{
    /// <summary>
    /// Validates the specified instance and returns the validation result.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <returns>The validation result.</returns>
    ValidationResult Validate(T instance);

    /// <summary>
    /// Validates the specified instance and throws when validation fails.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <param name="message">Optional custom exception message.</param>
    void ValidateAndThrow(T instance, string? message = null);
}
