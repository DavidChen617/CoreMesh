namespace CoreMesh.Validation;

/// <summary>
/// Defines validation rules for the current type.
/// </summary>
/// <typeparam name="T">The model type being validated.</typeparam>
public interface IValidatable<T>
{
    /// <summary>
    /// Configures validation rules for the current type.
    /// </summary>
    /// <param name="builder">The validation rule builder.</param>
    void ConfigureRules(ValidationBuilder<T> builder);
}
