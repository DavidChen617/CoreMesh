using System;
using System.Linq.Expressions;

namespace CoreMesh.Validation.Abstractions;

/// <summary>
/// Defines a fluent API for building validation rules for type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type being validated.</typeparam>
public interface IValidationBuilder<T>
{
    /// <summary>
    /// Starts building validation rules for the specified property.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="expression">A lambda expression that identifies the property to validate.</param>
    /// <returns>A <see cref="PropertyRuleBuilder{T, TProperty}"/> for chaining validation rules.</returns>
    PropertyRuleBuilder<T, TProperty> For<TProperty>(Expression<Func<T, TProperty>> expression);
}
