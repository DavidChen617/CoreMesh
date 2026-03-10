using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CoreMesh.Validation.Abstractions;

namespace CoreMesh.Validation;

/// <summary>
/// Provides a fluent API for building validation rules for type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type being validated.</typeparam>
public class ValidationBuilder<T> : IValidationBuilder<T>
{
    private readonly List<Func<T, RuleResult>> _rules = new();

    internal IReadOnlyList<Func<T, RuleResult>> Build() => _rules;

    /// <summary>
    /// Starts building validation rules for the specified property.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="expression">A lambda expression that identifies the property to validate.</param>
    /// <returns>A <see cref="PropertyRuleBuilder{T, TProperty}"/> for chaining validation rules.</returns>
    /// <exception cref="ArgumentException">Thrown when the expression is not a member expression.</exception>
    public PropertyRuleBuilder<T, TProperty> For<TProperty>(Expression<Func<T, TProperty>> expression)
    {
        Expression body = expression.Body;

        if (body is UnaryExpression u && u.NodeType == ExpressionType.Convert)
            body = u.Operand;

        if (body is not MemberExpression member)
            throw new ArgumentException("Expression must be a member expression.", nameof(expression));

        var name = member.Member.Name;
        var getter = expression.Compile();

        return new PropertyRuleBuilder<T, TProperty>(_rules, name, getter);
    }
}
