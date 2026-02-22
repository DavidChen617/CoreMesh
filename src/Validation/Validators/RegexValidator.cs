using System.Text.RegularExpressions;

namespace CoreMesh.Validation.Validators;

public sealed class RegexValidator : IPropertyValidator<string?>
{
    private string? _message;

    private readonly Regex _regex;

    public RegexValidator(string pattern)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pattern);
        _regex = new Regex(pattern, RegexOptions.Compiled);
    }

    public bool IsValid(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        return _regex.IsMatch(value);
    }

    public string GetErrorMessage(string propertyName)
    {
        return _message ?? $"'{propertyName}' format is invalid.";
    }


    public void SetMessage(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        _message = message;
    }
}
