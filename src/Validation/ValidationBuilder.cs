using System.Linq.Expressions;

namespace CoreMesh.Validation;

/// <summary>
/// Builds validation rules for a model type.
/// </summary>
/// <typeparam name="T">The model type.</typeparam>
public sealed class ValidationBuilder<T>
{
    private readonly List<IValidationRule<T>> _rules = [];

    /// <summary>
    /// Creates a rule builder for a model property.
    /// </summary>
    /// <typeparam name="TProperty">The property type.</typeparam>
    /// <param name="expression">The property access expression.</param>
    /// <returns>A fluent rule builder for the property.</returns>
    public RuleBuilder<T, TProperty> RuleFor<TProperty>(Expression<Func<T, TProperty>> expression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        
        var propertyName = GetPropertyName(expression);
        var propertyFunc = expression.Compile();
        
        var rule = new PropertyRule<T, TProperty>(propertyName, propertyFunc);
        AddRule(rule);

        return new RuleBuilder<T, TProperty>(rule);
    }

    /// <summary>
    /// Builds an executable validator from the configured rules.
    /// </summary>
    /// <returns>The object validator.</returns>
    public ObjectValidator<T> Build()
    {
        return new ObjectValidator<T>(_rules.ToArray());
    }

    internal void AddRule(IValidationRule<T> rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        _rules.Add(rule);
    }
    
    private static string GetPropertyName<TProperty>(Expression<Func<T, TProperty>> expression)
    {
        if (expression.Body is MemberExpression member)
        {
            return member.Member.Name;
        }

        throw new InvalidOperationException("RuleFor only supports simple member access expressions.");
    }
}
