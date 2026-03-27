namespace CoreMesh.Validation.Abstractions;

/// <summary>
/// Provides a fluent API for building validation rules for a specific property.
/// </summary>
/// <typeparam name="T">The type being validated.</typeparam>
/// <typeparam name="TProperty">The type of the property being validated.</typeparam>
/// <param name="rules">The list of rules to add to.</param>
/// <param name="name">The property name.</param>
/// <param name="getter">A function that retrieves the property value from the model.</param>
public class PropertyRuleBuilder<T, TProperty>(
    List<Func<T, RuleResult>> rules,
    string name,
    Func<T, TProperty> getter)
{
    /// <summary>
    /// Adds a custom validation rule using a predicate.
    /// </summary>
    /// <param name="predicate">A function that returns <c>true</c> if the value is valid.</param>
    /// <param name="message">The error message to use when validation fails.</param>
    /// <returns>The current builder for method chaining.</returns>
    public PropertyRuleBuilder<T, TProperty> Must(Func<TProperty, bool> predicate, string? message = null)
    {
        rules.Add(x =>
        {
            var value = getter(x);
            var ok = predicate(value);

            return ok
                ? new RuleResult(name, null)
                : new RuleResult(name, message ?? "Invalid value!");
        });

        return this;
    }

    /// <summary>
    /// Stops subsequent rule evaluation for this property if any previous rule has failed.
    /// </summary>
    /// <returns>The current builder for method chaining.</returns>
    /// <example>
    /// <code>
    /// builder.For(x => x.Name)
    ///     .NotNull()
    ///     .StopOnInvalid()  // If NotNull fails, MinLength won't execute
    ///     .MinLength(5);
    /// </code>
    /// </example>
    public PropertyRuleBuilder<T, TProperty> StopOnInvalid()
    {
        rules.Add(_ => new RuleResult(name, null, StopOnError: true));
        return this;
    }
}
