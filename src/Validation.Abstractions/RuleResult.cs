namespace CoreMesh.Validation.Abstractions;

/// <summary>
/// Represents the result of evaluating a single validation rule.
/// </summary>
/// <param name="PropertyName">The name of the property being validated.</param>
/// <param name="ErrorMessage">The error message if validation failed; otherwise, <c>null</c>.</param>
/// <param name="StopOnError">If <c>true</c>, stops subsequent rule evaluation for this property when any previous rule has failed.</param>
public readonly record struct RuleResult(
    string PropertyName,
    string? ErrorMessage,
    bool StopOnError = false);
