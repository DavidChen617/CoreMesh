using CoreMesh.Result.Exceptions.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CoreMesh.Result.Extensions;

public static class ResultServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
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
        public IApplicationBuilder UseCoreMeshHttp()
        {
            app.UseExceptionHandler();
            return app;
        }
    }
}
