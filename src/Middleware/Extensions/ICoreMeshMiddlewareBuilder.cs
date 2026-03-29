using CoreMesh.Middleware.Idempotency;
using Microsoft.Extensions.DependencyInjection;

namespace CoreMesh.Middleware.Extensions;

/// <summary>
/// Fluent builder for registering CoreMesh middleware components into the ASP.NET Core pipeline.
/// </summary>
public interface ICoreMeshMiddlewareBuilder
{
    /// <summary>
    /// Adds <see cref="IdempotencyMiddleware"/> to the pipeline.
    /// </summary>
    /// <param name="configure">Optional delegate to configure <see cref="IIdempotencyBuilder"/>.</param>
    ICoreMeshMiddlewareBuilder AddIdempotency(
        Action<IIdempotencyBuilder>? configure = null);
}

/// <summary>
/// Tracks the ordered list of middleware types registered via <see cref="ICoreMeshMiddlewareBuilder"/>.
/// </summary>
public class CoreMeshMiddlewareRegistry
{
    private readonly List<Type> _pipeline = new();
    internal void Register<T>() => _pipeline.Add(typeof(T));

    /// <summary>Gets the ordered middleware types to be added to the pipeline.</summary>
    public IReadOnlyList<Type> Pipeline => _pipeline;
}

/// <summary>
/// Default implementation of <see cref="ICoreMeshMiddlewareBuilder"/>.
/// Registers middleware types and their dependencies into the DI container.
/// </summary>
public class CoreMeshMiddlewareBuilder(IServiceCollection services, CoreMeshMiddlewareRegistry registry)
    : ICoreMeshMiddlewareBuilder
{
    /// <inheritdoc/>
    public ICoreMeshMiddlewareBuilder AddIdempotency(Action<IIdempotencyBuilder>? configure = null)
    {
        registry.Register<IdempotencyMiddleware>();

        var builder = new IdempotencyBuilder();
        configure?.Invoke(builder);
        services.AddSingleton(builder.Options);

        if (builder.IdempotencyHandlerType is not null)
            services.AddScoped(typeof(IIdempotencyHandler), builder.IdempotencyHandlerType);
        else if (builder.IdempotencyHandlerInstance is not null)
            services.AddSingleton<IIdempotencyHandler>(builder.IdempotencyHandlerInstance);
        else
            services.AddScoped<IIdempotencyHandler, DefaultIdempotencyHandler>();

        return this;
    }
}
