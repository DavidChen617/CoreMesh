using CoreMesh.Result.Exceptions.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CoreMesh.Result.Extensions;

/// <summary>
/// Extension methods for registering CoreMesh HTTP services and middleware.
/// </summary>
public static class ResultServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers ProblemDetails and CoreMesh exception handlers.
        /// </summary>
        public IServiceCollection AddCoreMeshExceptionHandling()
        {
            services
                .AddProblemDetails()
                .AddExceptionHandler<ValidationExceptionHandler>()
                .AddExceptionHandler<GlobalExceptionHandler>();

            return services;
        }
    }

    extension(IApplicationBuilder app)
    {
        /// <summary>
        /// Enables the CoreMesh exception handler middleware.
        /// </summary>
        public IApplicationBuilder UseCoreMeshExceptionHandling()
        {
            app.UseExceptionHandler();
            return app;
        }
    }
}
