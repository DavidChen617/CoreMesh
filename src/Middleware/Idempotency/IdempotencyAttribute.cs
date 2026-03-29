namespace CoreMesh.Middleware.Idempotency;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class IdempotencyAttribute : Attribute
{
    public string? CustomIdempotencyKeyName { get; set; }

    public IdempotencyAttribute()
    {
    }

    public IdempotencyAttribute(string customIdempotencyKeyName)
        => CustomIdempotencyKeyName = customIdempotencyKeyName;
}
