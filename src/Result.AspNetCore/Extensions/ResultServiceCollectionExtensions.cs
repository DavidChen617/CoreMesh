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
        /// Registers ProblemDetails and all CoreMesh exception handlers.
        /// </summary>
        public IServiceCollection AddCoreMeshHttp()
        {
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
        /// Enables the CoreMesh exception handler middleware.
        /// </summary>
        public IApplicationBuilder UseCoreMeshHttp()
        {
            app.UseExceptionHandler();
            return app;
        }
    }
}
