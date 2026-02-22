namespace CoreMesh.Validation;

public interface IValidatable<T>
{
    void ConfigureRules(ValidationBuilder<T> builder);
}
