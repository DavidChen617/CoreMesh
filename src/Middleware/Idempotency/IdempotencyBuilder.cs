namespace CoreMesh.Middleware.Idempotency;

public class IdempotencyBuilder : IIdempotencyBuilder
{
    public IdempotencyOptions Options { get; } = new();
    public Type? IdempotencyHandlerType { get; private set; }
    public IIdempotencyHandler? IdempotencyHandlerInstance { get; private set; }

    public IIdempotencyBuilder Configure(Action<IdempotencyOptions> configure)
    {
        configure?.Invoke(Options);
        return this;
    }

    public IIdempotencyBuilder WithHandler<THandler>() where THandler : class, IIdempotencyHandler
    {
        IdempotencyHandlerType = typeof(THandler);
        IdempotencyHandlerInstance = null;
        return this;
    }

    public IIdempotencyBuilder WithHandler(IIdempotencyHandler handler)
    {
        IdempotencyHandlerInstance = handler;
        IdempotencyHandlerType = null;
        return this;
    }
}
