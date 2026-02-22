namespace CoreMesh.Validation;

public sealed class ValidationFailure(
    string propertyName,
    string errorMessage,
    object? attemptedValue = null)
{
    public string PropertyName { get; } = propertyName;
    public string ErrorMessage { get; } = errorMessage;
    public object? AttemptedValue { get; } = attemptedValue;
}
