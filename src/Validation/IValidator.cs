namespace CoreMesh.Validation;

/// <summary>
/// Defines the contract for a validator that can validate objects.
/// </summary>
public interface IValidator
{
    /// <summary>
    /// Validates the specified model.
    /// </summary>
    /// <typeparam name="T">The type of the model to validate.</typeparam>
    /// <param name="model">The model instance to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> containing the validation outcome.</returns>
    ValidationResult Validate<T>(T model);
}