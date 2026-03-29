using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CoreMesh.Middleware.Extensions;

public static class MiddlewareExtension
{
    extension(IServiceCollection services)
    {
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
        public IApplicationBuilder UseCoreMeshMiddleware()
        {
            var registry = app.ApplicationServices.GetRequiredService<CoreMeshMiddlewareRegistry>();

            foreach (var type in registry.Pipeline)
                app.UseMiddleware(type);

            return app;
        }
    }
}
