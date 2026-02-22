namespace CoreMesh.Validation;

public interface IPropertyValidator<in TProperty>
{
    bool IsValid(TProperty value);
    string GetErrorMessage(string propertyName);
}
