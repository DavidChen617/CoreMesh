namespace CoreMesh.Validation;

/// <summary>
/// Represents a single validation failure.
/// </summary>
/// <param name="propertyName">The failed property name.</param>
/// <param name="errorMessage">The validation error message.</param>
/// <param name="attemptedValue">The attempted property value.</param>
public sealed class ValidationFailure(
    string propertyName,
    string errorMessage,
    object? attemptedValue = null)
{
    /// <summary>
    /// Gets the property name that failed validation.
    /// </summary>
    public string PropertyName { get; } = propertyName;

    /// <summary>
    /// Gets the validation error message.
    /// </summary>
    public string ErrorMessage { get; } = errorMessage;

    /// <summary>
    /// Gets the value that failed validation.
    /// </summary>
    public object? AttemptedValue { get; } = attemptedValue;
}
