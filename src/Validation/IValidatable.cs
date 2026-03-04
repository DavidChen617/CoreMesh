namespace CoreMesh.Validation;

/// <summary>
/// Defines a contract for types that can configure their own validation rules.
/// </summary>
/// <typeparam name="T">The type being validated.</typeparam>
public interface IValidatable<T>
{
    /// <summary>
    /// Configures the validation rules for type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="builder">The validation builder used to define rules.</param>
    void ConfigureValidateRules(ValidationBuilder<T> builder);
}
