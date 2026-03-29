using Microsoft.AspNetCore.Http;

namespace CoreMesh.Middleware.Idempotency;

/// <summary>
/// Configuration options for <see cref="IdempotencyMiddleware"/>.
/// </summary>
public class IdempotencyOptions
{
    /// <summary>
    /// The request header name that carries the idempotency key.
    /// Defaults to <c>Idempotency-Key</c>.
    /// </summary>
    public string IdempotencyKeyName { get; set; } = "Idempotency-Key";

    /// <summary>
    /// The response header name added when a cached response is replayed.
    /// Defaults to <c>X-Idempotency-Replayed</c>.
    /// </summary>
    public string IdempotencyHeaderReplayed { get; set; } = "X-Idempotency-Replayed";

    /// <summary>
    /// How long a stored response remains valid.
    /// Defaults to 24 hours.
    /// </summary>
    public TimeSpan CacheExpiry { get; set; } = TimeSpan.FromHours(24);

    /// <summary>
    /// Optional factory for customising the 400 error response body when the
    /// idempotency key header is missing.
    /// Receives the error message and current <see cref="HttpContext"/>;
    /// must return a JSON string.
    /// When <see langword="null"/>, a default <c>{ "error": "..." }</c> payload is used.
    /// </summary>
    public Func<string, HttpContext, string>? ErrorResponseFormatter { get; set; }
}
