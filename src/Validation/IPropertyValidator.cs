namespace CoreMesh.Validation;

/// <summary>
/// Represents a validator for a single property value.
/// </summary>
/// <typeparam name="TProperty">The property type.</typeparam>
public interface IPropertyValidator<in TProperty>
{
    /// <summary>
    /// Determines whether the value is valid.
    /// </summary>
    /// <param name="value">The property value.</param>
    /// <returns><see langword="true"/> when valid; otherwise <see langword="false"/>.</returns>
    bool IsValid(TProperty value);

    /// <summary>
    /// Gets the error message for the current validator.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The formatted error message.</returns>
    string GetErrorMessage(string propertyName);
}
