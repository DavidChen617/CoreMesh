using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CoreMesh.Endpoints.Extensions;

/// <summary>
/// Provides endpoint registration and mapping extension methods.
/// </summary>
public static class EndpointExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Scans loaded assemblies and registers endpoint implementations in the service collection.
        /// </summary>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddEndpoints()
        {
            var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => !p.IsInterface && !p.IsAbstract);

            var endpoints = allTypes
                .Where(p => typeof(IEndpoint).IsAssignableFrom(p));

            var groupEndpoints = allTypes
                .Where(p => typeof(IGroupEndpoint).IsAssignableFrom(p));

            var groupedEndpoints = allTypes
                .Where(p => p.GetInterfaces().Any(i =>
                    i.IsGenericType && (
                        i.GetGenericTypeDefinition() == typeof(IGroupedEndpoint<>)
                        || i.GetGenericTypeDefinition() == typeof(IGroupedEndpoint<,>)
                        || i.GetGenericTypeDefinition() == typeof(IGroupedEndpoint<,,>))));

            foreach (var endpoint in endpoints)
            {
                services.AddSingleton(typeof(IEndpoint), endpoint);
            }

            foreach (var groupEndpoint in groupEndpoints)
            {
                services.AddSingleton(typeof(IGroupEndpoint), groupEndpoint);
            }

            foreach (var groupedEndpoint in groupedEndpoints)
            {
                services.AddSingleton(typeof(IGroupedEndpoint), groupedEndpoint);
            }

            return services;
        }
    }

    extension(WebApplication app)
    {
        /// <summary>
        /// Maps registered endpoints and grouped endpoints to the web application.
        /// </summary>
        /// <returns>The web application.</returns>
        public WebApplication MapEndpoints()
        {
            var endpoints = app.Services.GetServices<IEndpoint>();
            var groupEndpoints = app.Services.GetServices<IGroupEndpoint>().ToList();
            var groupedEndpoints = app.Services.GetServices<IGroupedEndpoint>().ToList();

            foreach (var endpoint in endpoints)
            {
                endpoint.AddRoute(app);
            }

            foreach (var groupEndpoint in groupEndpoints)
            {
                var group = app.MapGroup(groupEndpoint.GroupPrefix);
                groupEndpoint.Configure(group);

                foreach (var groupedEndpoint in groupedEndpoints.Where(x =>
                             x.GroupType == groupEndpoint.GetType()))
                {
                    groupedEndpoint.AddRoute(group);
                }
            }

            return app;
        }
    }
}
