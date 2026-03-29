using Microsoft.AspNetCore.Http;

namespace CoreMesh.Middleware.Idempotency;

public class IdempotencyOptions
{
    public string IdempotencyKeyName { get; set; } = "Idempotency-Key";
    public string IdempotencyHeaderReplayed { get; set; } = "X-Idempotency-Replayed";
    public TimeSpan CacheExpiry { get; set; } = TimeSpan.FromHours(24);
    public Func<string, HttpContext, string>? ErrorResponseFormatter { get; set; }
}
