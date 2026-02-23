using System.Text.RegularExpressions;

namespace CoreMesh.Validation.Validators;

/// <summary>
/// Provides property validation logic for this rule type.
/// </summary>
public sealed class RegexValidator : IPropertyValidator<string?>
{

    private readonly Regex _regex;
    /// <summary>
    /// Initializes a new instance of the validator.
    /// </summary>

    public RegexValidator(string pattern)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pattern);
        _regex = new Regex(pattern, RegexOptions.Compiled);
    }
    /// <summary>
    /// Determines whether the value is valid.
    /// </summary>

    public bool IsValid(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        return _regex.IsMatch(value);
    }
    /// <summary>
    /// Gets the error message for a failed validation.
    /// </summary>

    public string GetErrorMessage(string propertyName)
    {
        return $"'{propertyName}' format is invalid.";
    }
}
