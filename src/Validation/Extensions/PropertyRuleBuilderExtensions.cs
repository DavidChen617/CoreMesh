namespace CoreMesh.Validation.Extensions;

/// <summary>
/// Provides extension methods for <see cref="PropertyRuleBuilder{T, TProperty}"/> with common validation rules.
/// </summary>
public static class PropertyRuleBuilderExtensions
{
    /// <summary>
    /// Validates that a reference type value is not null.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <typeparam name="TProperty">The property type.</typeparam>
    /// <param name="b">The property rule builder.</param>
    /// <param name="message">Custom error message.</param>
    /// <returns>The builder for method chaining.</returns>
    public static PropertyRuleBuilder<T, TProperty> NotNull<T, TProperty>(
        this PropertyRuleBuilder<T, TProperty> b,
        string? message = null)
        where TProperty : class?
        => b.Must(v => v is not null, message ?? "Must not be null.");

    /// <summary>
    /// Validates that a nullable value type has a value.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <typeparam name="TProperty">The property type.</typeparam>
    /// <param name="b">The property rule builder.</param>
    /// <param name="message">Custom error message.</param>
    /// <returns>The builder for method chaining.</returns>
    public static PropertyRuleBuilder<T, TProperty?> NotNull<T, TProperty>(
        this PropertyRuleBuilder<T, TProperty?> b,
        string? message = null)
        where TProperty : struct
        => b.Must(v => v.HasValue, message ?? "Must not be null.");

    /// <summary>
    /// Validates that a string value is not null, empty, or whitespace.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <typeparam name="TProperty">The property type.</typeparam>
    /// <param name="b">The property rule builder.</param>
    /// <param name="message">Custom error message.</param>
    /// <returns>The builder for method chaining.</returns>
    public static PropertyRuleBuilder<T, TProperty> NotEmpty<T, TProperty>(
        this PropertyRuleBuilder<T, TProperty> b,
        string? message = null)
        where TProperty : class?
        => b.Must(s => s is string str ? !string.IsNullOrWhiteSpace(str) : s is not null,
            message ?? "Must not be empty.");

    /// <summary>
    /// Validates that a string has a minimum length.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <typeparam name="TProperty">The property type.</typeparam>
    /// <param name="b">The property rule builder.</param>
    /// <param name="min">The minimum required length.</param>
    /// <param name="message">Custom error message.</param>
    /// <returns>The builder for method chaining.</returns>
    public static PropertyRuleBuilder<T, TProperty> MinLength<T, TProperty>(
        this PropertyRuleBuilder<T, TProperty> b,
        int min,
        string? message = null)
        where TProperty : class?
        => b.Must(s => s is string str && str.Length >= min, message ?? $"Length must be >= {min}.");

    /// <summary>
    /// Validates that a string does not exceed a maximum length.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <typeparam name="TProperty">The property type.</typeparam>
    /// <param name="b">The property rule builder.</param>
    /// <param name="max">The maximum allowed length.</param>
    /// <param name="message">Custom error message.</param>
    /// <returns>The builder for method chaining.</returns>
    public static PropertyRuleBuilder<T, TProperty> MaxLength<T, TProperty>(
        this PropertyRuleBuilder<T, TProperty> b,
        int max,
        string? message = null)
        where TProperty : class?
        => b.Must(s => s is not string str || str.Length <= max, message ?? $"Length must be <= {max}.");

    /// <summary>
    /// Validates that a string is a valid email address format.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <typeparam name="TProperty">The property type.</typeparam>
    /// <param name="b">The property rule builder.</param>
    /// <param name="message">Custom error message.</param>
    /// <returns>The builder for method chaining.</returns>
    public static PropertyRuleBuilder<T, TProperty> EmailAddress<T, TProperty>(
        this PropertyRuleBuilder<T, TProperty> b,
        string? message = null)
        where TProperty : class?
        => b.Must(s => s is not string str || System.Text.RegularExpressions.Regex.IsMatch(str,
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$"), message ?? "Invalid email format.");

    /// <summary>
    /// Validates that a value is greater than a specified minimum.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <typeparam name="TProperty">The property type (must implement <see cref="IComparable{T}"/>).</typeparam>
    /// <param name="b">The property rule builder.</param>
    /// <param name="min">The value must be greater than this.</param>
    /// <param name="message">Custom error message.</param>
    /// <returns>The builder for method chaining.</returns>
    public static PropertyRuleBuilder<T, TProperty> GreaterThan<T, TProperty>(
        this PropertyRuleBuilder<T, TProperty> b,
        TProperty min,
        string? message = null)
        where TProperty : IComparable<TProperty>
        => b.Must(v => v.CompareTo(min) > 0, message ?? $"Must be greater than {min}.");

    /// <summary>
    /// Validates that a value is greater than or equal to a specified minimum.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <typeparam name="TProperty">The property type (must implement <see cref="IComparable{T}"/>).</typeparam>
    /// <param name="b">The property rule builder.</param>
    /// <param name="min">The minimum allowed value (inclusive).</param>
    /// <param name="message">Custom error message.</param>
    /// <returns>The builder for method chaining.</returns>
    public static PropertyRuleBuilder<T, TProperty> GreaterThanOrEqual<T, TProperty>(
        this PropertyRuleBuilder<T, TProperty> b,
        TProperty min,
        string? message = null)
        where TProperty : IComparable<TProperty>
        => b.Must(v => v.CompareTo(min) >= 0, message ?? $"Must be >= {min}.");

    /// <summary>
    /// Validates that a value is less than a specified maximum.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <typeparam name="TProperty">The property type (must implement <see cref="IComparable{T}"/>).</typeparam>
    /// <param name="b">The property rule builder.</param>
    /// <param name="max">The value must be less than this.</param>
    /// <param name="message">Custom error message.</param>
    /// <returns>The builder for method chaining.</returns>
    public static PropertyRuleBuilder<T, TProperty> LessThan<T, TProperty>(
        this PropertyRuleBuilder<T, TProperty> b,
        TProperty max,
        string? message = null)
        where TProperty : IComparable<TProperty>
        => b.Must(v => v.CompareTo(max) < 0, message ?? $"Must be less than {max}.");

    /// <summary>
    /// Validates that a value is less than or equal to a specified maximum.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <typeparam name="TProperty">The property type (must implement <see cref="IComparable{T}"/>).</typeparam>
    /// <param name="b">The property rule builder.</param>
    /// <param name="max">The maximum allowed value (inclusive).</param>
    /// <param name="message">Custom error message.</param>
    /// <returns>The builder for method chaining.</returns>
    public static PropertyRuleBuilder<T, TProperty> LessThanOrEqual<T, TProperty>(
        this PropertyRuleBuilder<T, TProperty> b,
        TProperty max,
        string? message = null)
        where TProperty : IComparable<TProperty>
        => b.Must(v => v.CompareTo(max) <= 0, message ?? $"Must be <= {max}.");

    /// <summary>
    /// Validates that a nullable value type is greater than or equal to a specified minimum.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <typeparam name="TProperty">The property type (must implement <see cref="IComparable{T}"/>).</typeparam>
    /// <param name="b">The property rule builder.</param>
    /// <param name="min">The minimum allowed value (inclusive).</param>
    /// <param name="message">Custom error message.</param>
    /// <returns>The builder for method chaining.</returns>
    public static PropertyRuleBuilder<T, TProperty?> GreaterThanOrEqual<T, TProperty>(
        this PropertyRuleBuilder<T, TProperty?> b,
        TProperty min,
        string? message = null)
        where TProperty : struct, IComparable<TProperty>
        => b.Must(v => !v.HasValue || v.Value.CompareTo(min) >= 0, message ?? $"Must be >= {min}.");

    /// <summary>
    /// Validates that a nullable value type is less than or equal to a specified maximum.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <typeparam name="TProperty">The property type (must implement <see cref="IComparable{T}"/>).</typeparam>
    /// <param name="b">The property rule builder.</param>
    /// <param name="max">The maximum allowed value (inclusive).</param>
    /// <param name="message">Custom error message.</param>
    /// <returns>The builder for method chaining.</returns>
    public static PropertyRuleBuilder<T, TProperty?> LessThanOrEqual<T, TProperty>(
        this PropertyRuleBuilder<T, TProperty?> b,
        TProperty max,
        string? message = null)
        where TProperty : struct, IComparable<TProperty>
        => b.Must(v => !v.HasValue || v.Value.CompareTo(max) <= 0, message ?? $"Must be <= {max}.");

    /// <summary>
    /// Validates that a list is not null.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <typeparam name="TItem">The type of items in the list.</typeparam>
    /// <param name="b">The property rule builder.</param>
    /// <param name="message">Custom error message.</param>
    /// <returns>The builder for method chaining.</returns>
    public static PropertyRuleBuilder<T, List<TItem>?> NotNull<T, TItem>(
        this PropertyRuleBuilder<T, List<TItem>?> b,
        string? message = null)
        => b.Must(c => c is not null, message ?? "Must not be null.");

    /// <summary>
    /// Validates that a list is not null and contains at least one item.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <typeparam name="TItem">The type of items in the list.</typeparam>
    /// <param name="b">The property rule builder.</param>
    /// <param name="message">Custom error message.</param>
    /// <returns>The builder for method chaining.</returns>
    public static PropertyRuleBuilder<T, List<TItem>?> NotEmpty<T, TItem>(
        this PropertyRuleBuilder<T, List<TItem>?> b,
        string? message = null)
        => b.Must(c => c != null && c.Count > 0, message ?? "Collection must not be empty.");
}
