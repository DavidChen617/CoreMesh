namespace CoreMesh.Middleware.Idempotency;

/// <summary>
/// Marks a Minimal API endpoint or MVC controller action as requiring idempotency enforcement.
/// When applied, <see cref="IdempotencyMiddleware"/> intercepts POST requests and replays
/// cached responses for duplicate idempotency keys.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class IdempotencyAttribute : Attribute
{
    /// <summary>
    /// Gets the custom request header name used to read the idempotency key.
    /// When <see langword="null"/>, falls back to <see cref="IdempotencyOptions.IdempotencyKeyName"/>.
    /// </summary>
    public string? CustomIdempotencyKeyName { get; set; }

    /// <summary>
    /// Initialises the attribute using the default idempotency key header name.
    /// </summary>
    public IdempotencyAttribute()
    {
    }

    /// <summary>
    /// Initialises the attribute with a custom idempotency key header name.
    /// </summary>
    /// <param name="customIdempotencyKeyName">The request header name that carries the idempotency key.</param>
    public IdempotencyAttribute(string customIdempotencyKeyName)
        => CustomIdempotencyKeyName = customIdempotencyKeyName;
}
