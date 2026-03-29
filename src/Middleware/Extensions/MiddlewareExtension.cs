using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CoreMesh.Middleware.Extensions;

/// <summary>
/// Extension methods for registering and using CoreMesh middleware.
/// </summary>
public static class MiddlewareExtension
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers CoreMesh middleware components into the DI container.
        /// Call <c>app.UseCoreMeshMiddleware()</c> to add them to the request pipeline.
        /// </summary>
        /// <param name="configure">Optional delegate to add middleware such as idempotency.</param>
        public IServiceCollection AddCoreMeshMiddleware(Action<ICoreMeshMiddlewareBuilder>? configure = null)
        {
            var registry = new CoreMeshMiddlewareRegistry();
            services.AddSingleton(registry);

            var builder = new CoreMeshMiddlewareBuilder(services, registry);
            configure?.Invoke(builder);

            return services;
        }
    }

    extension(IApplicationBuilder app)
    {
        /// <summary>
        /// Adds all middleware registered via <c>AddCoreMeshMiddleware</c> to the request pipeline
        /// in the order they were registered.
        /// </summary>
        public IApplicationBuilder UseCoreMeshMiddleware()
        {
            var registry = app.ApplicationServices.GetRequiredService<CoreMeshMiddlewareRegistry>();

            foreach (var type in registry.Pipeline)
                app.UseMiddleware(type);

            return app;
        }
    }
}
