using System.Linq.Expressions;

namespace CoreMesh.Validation;

public sealed class ValidationBuilder<T>
{
    private readonly List<IValidationRule<T>> _rules = [];

    public RuleBuilder<T, TProperty> RuleFor<TProperty>(Expression<Func<T, TProperty>> expression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        
        var propertyName = GetPropertyName(expression);
        var propertyFunc = expression.Compile();
        
        var rule = new PropertyRule<T, TProperty>(propertyName, propertyFunc);
        AddRule(rule);

        return new RuleBuilder<T, TProperty>(rule);
    }

    public ObjectValidator<T> Build()
    {
        return new ObjectValidator<T>(_rules.ToArray());
    }

    internal void AddRule(IValidationRule<T> rule) => _rules.Add(rule);
    
    private static string GetPropertyName<TProperty>(Expression<Func<T, TProperty>> expression)
    {
        if (expression.Body is MemberExpression member)
        {
            return member.Member.Name;
        }

        throw new InvalidOperationException("RuleFor only supports simple member access expressions.");
    }
}
