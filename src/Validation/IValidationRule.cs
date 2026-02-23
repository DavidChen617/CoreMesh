namespace CoreMesh.Validation;

/// <summary>
/// Represents a validation rule that can validate an instance.
/// </summary>
/// <typeparam name="T">The model type.</typeparam>
public interface IValidationRule<in T>
{
    /// <summary>
    /// Validates the specified instance and appends failures to the provided list.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <param name="failures">The failure collection.</param>
    void Validate(T instance, List<ValidationFailure> failures);
}
