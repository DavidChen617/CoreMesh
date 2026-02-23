namespace CoreMesh.Validation;

/// <summary>
/// Represents the result of a validation operation.
/// </summary>
public sealed class ValidationResult
{
    /// <summary>
    /// Initializes a new empty validation result.
    /// </summary>
    public ValidationResult()
    {
        Errors = [];
    }

    /// <summary>
    /// Initializes a new validation result with the specified failures.
    /// </summary>
    /// <param name="errors">The validation failures.</param>
    public ValidationResult(IEnumerable<ValidationFailure> errors)
    {
        Errors = errors.Where(x => x is not null).ToList();
    }

    /// <summary>
    /// Gets the validation failures.
    /// </summary>
    public List<ValidationFailure> Errors { get; }

    /// <summary>
    /// Gets a value indicating whether validation succeeded.
    /// </summary>
    public bool IsValid => Errors.Count == 0;
    
    /// <summary>
    /// Throws a <see cref="ValidationException"/> when the result is invalid.
    /// </summary>
    /// <param name="message">Optional custom exception message.</param>
    public void ThrowIfInvalid(string? message = null)
    {
        if (IsValid)
        {
            return;
        }

        throw new ValidationException(message ?? string.Join("; ", Errors.Select(x => x.ErrorMessage)));
    }
}
