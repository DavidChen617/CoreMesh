namespace CoreMesh.Middleware.Idempotency;

/// <summary>
/// Default implementation of <see cref="IIdempotencyBuilder"/>.
/// Collects configuration that is applied during DI registration.
/// </summary>
public class IdempotencyBuilder : IIdempotencyBuilder
{
    /// <summary>Gets the configured <see cref="IdempotencyOptions"/>.</summary>
    public IdempotencyOptions Options { get; } = new();

    /// <summary>Gets the handler type to register, or <see langword="null"/> if an instance is used.</summary>
    public Type? IdempotencyHandlerType { get; private set; }

    /// <summary>Gets the handler instance to register, or <see langword="null"/> if a type is used.</summary>
    public IIdempotencyHandler? IdempotencyHandlerInstance { get; private set; }

    /// <inheritdoc/>
    public IIdempotencyBuilder Configure(Action<IdempotencyOptions> configure)
    {
        configure?.Invoke(Options);
        return this;
    }

    /// <inheritdoc/>
    public IIdempotencyBuilder WithHandler<THandler>() where THandler : class, IIdempotencyHandler
    {
        IdempotencyHandlerType = typeof(THandler);
        IdempotencyHandlerInstance = null;
        return this;
    }

    /// <inheritdoc/>
    public IIdempotencyBuilder WithHandler(IIdempotencyHandler handler)
    {
        IdempotencyHandlerInstance = handler;
        IdempotencyHandlerType = null;
        return this;
    }
}
