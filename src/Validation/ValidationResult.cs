using System.ComponentModel.DataAnnotations;

namespace CoreMesh.Validation;

public sealed class ValidationResult
{
    public ValidationResult()
    {
        Errors = [];
    }

    public ValidationResult(IEnumerable<ValidationFailure> errors)
    {
        Errors = errors.Where(x => x is not null).ToList();
    }

    public List<ValidationFailure> Errors { get; }
    public bool IsValid => Errors.Count == 0;
    
    public void ThrowIfInvalid(string? message = null)
    {
        if (IsValid)
        {
            return;
        }

        // TODO 
        throw new ValidationException(message ?? string.Join("; ", Errors.Select(x => x.ErrorMessage)));
    }
}
