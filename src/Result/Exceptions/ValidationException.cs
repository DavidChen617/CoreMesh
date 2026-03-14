using System.Net;

namespace CoreMesh.Result.Exceptions;

/// <summary>
/// Represents a validation exception containing grouped field errors.
/// </summary>
public sealed class ValidationException : AppException
{
    /// <summary>
    /// Gets the grouped validation errors keyed by field name.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Errors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class from grouped errors.
    /// </summary>
    /// <param name="errors">The grouped validation errors.</param>
    public ValidationException(IDictionary<string, IReadOnlyList<string>> errors) :
        base("One or more validation errors occurred.", HttpStatusCode.UnprocessableEntity, "validation_error")
    {
        ArgumentNullException.ThrowIfNull(errors);
        Errors = errors.AsReadOnly();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class from a single field error.
    /// </summary>
    /// <param name="field">The field name.</param>
    /// <param name="error">The error message.</param>
    public ValidationException(string field, string error) :
        base("One or more validation errors occurred.", HttpStatusCode.UnprocessableEntity, "validation_error")
    {
        Errors = new Dictionary<string, IReadOnlyList<string>>
        {
            { field, [error] }
        }.AsReadOnly();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class from validation error items.
    /// </summary>
    /// <param name="failures">The validation error items.</param>
    public ValidationException(IEnumerable<ValidationErrorItem> failures) :
        base("One or more validation errors occurred.", HttpStatusCode.UnprocessableEntity, "validation_error")
    {
        Errors = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<string>)g.Select(f => f.ErrorMessage).ToList())
            .AsReadOnly();
    }
}

/// <summary>
/// Represents a single validation error item.
/// </summary>
/// <param name="PropertyName">The property name.</param>
/// <param name="ErrorMessage">The error message.</param>
public sealed record ValidationErrorItem(string PropertyName, string ErrorMessage);
