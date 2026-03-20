using CoreMesh.Middleware.Idempotency;
using Microsoft.Extensions.DependencyInjection;

namespace CoreMesh.Middleware.Extensions;

public interface ICoreMeshMiddlewareBuilder
{
    ICoreMeshMiddlewareBuilder AddIdempotency(
        Action<IIdempotencyBuilder>? configure = null);
}

public class CoreMeshMiddlewareRegistry
{
    private readonly List<Type> _pipeline = new();
    internal void Register<T>() => _pipeline.Add(typeof(T));
    public IReadOnlyList<Type> Pipeline => _pipeline;
}

public class CoreMeshMiddlewareBuilder(IServiceCollection services, CoreMeshMiddlewareRegistry registry)
    : ICoreMeshMiddlewareBuilder
{
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
