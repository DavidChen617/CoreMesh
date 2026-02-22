using System.Collections.Concurrent;

namespace CoreMesh.Validation;

public sealed class Validator<T> : IValidator<T> where T : IValidatable<T>
{
    private static readonly ConcurrentDictionary<Type, object> Cache = new();
    
    public ValidationResult Validate(T instance) 
    {
        ArgumentNullException.ThrowIfNull(instance);

        var objectValidator = (ObjectValidator<T>)Cache.GetOrAdd(typeof(T), _ =>
        {
            var builder = new ValidationBuilder<T>();
            instance.ConfigureRules(builder); 
            return builder.Build();
        });

        return objectValidator.Validate(instance);
    }

    public void ValidateAndThrow(T instance, string? message = null)
    {
        Validate(instance).ThrowIfInvalid(message);
    }
}
