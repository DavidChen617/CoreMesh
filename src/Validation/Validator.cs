using System.Collections.Concurrent;

namespace CoreMesh.Validation;

/// <summary>
/// Default validator entry point for a validatable model type.
/// </summary>
/// <typeparam name="T">The model type.</typeparam>
public sealed class Validator<T> : IValidator<T> where T : IValidatable<T>
{
    private static readonly ConcurrentDictionary<Type, object> Cache = new();
    
    /// <summary>
    /// Validates the specified instance.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <returns>The validation result.</returns>
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

    /// <summary>
    /// Validates the specified instance and throws when validation fails.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <param name="message">Optional custom exception message.</param>
    public void ValidateAndThrow(T instance, string? message = null)
    {
        Validate(instance).ThrowIfInvalid(message);
    }
}
