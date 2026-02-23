using CoreMesh.Http.Exceptions.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CoreMesh.Http.Extensions;

/// <summary>
/// Provides ASP.NET Core registration extensions for <c>CoreMesh.Http</c>.
/// </summary>
public static class HttpServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers CoreMesh HTTP services and exception handlers.
        /// </summary>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddCoreMeshHttp()
        {
            ArgumentNullException.ThrowIfNull(services);
            
            services
                .AddProblemDetails()
                .AddExceptionHandler<ValidationExceptionHandler>()
                .AddExceptionHandler<ConflictExceptionHandler>()
                .AddExceptionHandler<ForbiddenExceptionHandler>()
                .AddExceptionHandler<NotFoundExceptionHandler>()
                .AddExceptionHandler<GlobalExceptionHandler>();

            return services;
        }
    }

    extension(IApplicationBuilder app)
    {
        /// <summary>
        /// Enables CoreMesh global exception handling for the application pipeline.
        /// </summary>
        /// <returns>The application builder.</returns>
        public IApplicationBuilder UseCoreMeshHttp()
        {
            ArgumentNullException.ThrowIfNull(app);

            app.UseExceptionHandler();
            
            return app;
        }
    }
}
