namespace CoreMesh.Validation;

/// <summary>
/// Represents an exception thrown when validation fails.
/// </summary>
public sealed class ValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public ValidationException(string message)
        : base(message)
    {
    }
}
