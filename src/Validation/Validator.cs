using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace CoreMesh.Validation;

/// <summary>
/// Default implementation of <see cref="IValidator"/> that validates objects using configured rules.
/// </summary>
/// <remarks>
/// <para>Rules are cached per type for optimal performance.</para>
/// <para>Supports two modes:</para>
/// <list type="bullet">
///   <item>Model implements <see cref="IValidatable{T}"/> directly</item>
///   <item>Separate validator class resolved from DI</item>
/// </list>
/// </remarks>
/// <param name="sp">The service provider for resolving <see cref="IValidatable{T}"/> implementations.</param>
public sealed class Validator(IServiceProvider sp) : IValidator
{
    private readonly ConcurrentDictionary<Type, object> _rulesCache = new();

    /// <inheritdoc />
    public ValidationResult Validate<T>(T model)
    {
        if (model is null)
            throw new ArgumentNullException(nameof(model));

        var result = new ValidationResult();
        var stoppedProperties = new HashSet<string>();

        foreach (var rule in GetRules<T>(model))
        {
            var ruleResult = rule(model);

            // If the property has been marked as discontinued, subsequent rules are skipped
            if (stoppedProperties.Contains(ruleResult.PropertyName))
                continue;

            // If it is a stop mark, check this property to see if there are any errors
            if (ruleResult.StopOnError)
            {
                if (result.Errors.ContainsKey(ruleResult.PropertyName))
                    stoppedProperties.Add(ruleResult.PropertyName);
                continue;
            }

            // Handle errors normally
            if (ruleResult.ErrorMessage is null)
                continue;

            if (!result.Errors.TryGetValue(ruleResult.PropertyName, out var list))
                result.Errors[ruleResult.PropertyName] = list = new List<string>();

            list.Add(ruleResult.ErrorMessage);
        }

        return result;
    }

    private IReadOnlyList<Func<T, RuleResult>> GetRules<T>(T model)
    {
        var rules = (IReadOnlyList<Func<T, RuleResult>>)_rulesCache.GetOrAdd(typeof(T), _ =>
        {
            if (model is not IValidatable<T> validatable)
                validatable = sp.GetRequiredService<IValidatable<T>>();
            var b = new ValidationBuilder<T>();
            validatable.ConfigureValidateRules(b);
            return b.Build();
        });

        return rules;
    }
}
