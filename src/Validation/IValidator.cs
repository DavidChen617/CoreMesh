namespace CoreMesh.Validation;

public interface IValidator<T> where T : IValidatable<T>
{
    ValidationResult Validate(T instance);

    void ValidateAndThrow(T instance, string? message = null);
}
