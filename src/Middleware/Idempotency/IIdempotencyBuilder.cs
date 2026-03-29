namespace CoreMesh.Middleware.Idempotency;

/// <summary>
/// Fluent builder for configuring idempotency middleware behaviour.
/// </summary>
public interface IIdempotencyBuilder
{
    /// <summary>
    /// Configures idempotency options such as header name and cache expiry.
    /// </summary>
    /// <param name="configure">A delegate to configure <see cref="IdempotencyOptions"/>.</param>
    IIdempotencyBuilder Configure(Action<IdempotencyOptions> configure);

    /// <summary>
    /// Registers a custom <see cref="IIdempotencyHandler"/> implementation by type.
    /// The handler is resolved from the DI container per request (scoped).
    /// </summary>
    /// <typeparam name="THandler">The handler type to register.</typeparam>
    IIdempotencyBuilder WithHandler<THandler>()
        where THandler : class, IIdempotencyHandler;

    /// <summary>
    /// Registers a custom <see cref="IIdempotencyHandler"/> using a pre-built instance (singleton).
    /// </summary>
    /// <param name="handler">The handler instance to use.</param>
    IIdempotencyBuilder WithHandler(IIdempotencyHandler handler);
}
