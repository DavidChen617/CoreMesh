namespace CoreMesh.Validation;

public interface IValidationRule<in T>
{
    void Validate(T instance, List<ValidationFailure> failures);
}
