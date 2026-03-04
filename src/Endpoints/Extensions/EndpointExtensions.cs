using System.Reflection;
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
        public IServiceCollection AddEndpoints(params Assembly[] assemblies)
        {
            if (assemblies.Length == 0)
                throw new ArgumentException("At least one assembly must be provided.", nameof(assemblies));

            var types = assemblies
                .SelectMany(t => t.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract && !t.IsInterface).ToList();

            foreach (var endpoint in types.Where(t => typeof(IEndpoint).IsAssignableFrom(t)))
                services.AddSingleton(typeof(IEndpoint), endpoint);

            var registeredGroups = new HashSet<Type>();

            foreach (var groupType in types.Where(t => typeof(IGroupedEndpoint).IsAssignableFrom(t)))
            {
                services.AddSingleton(typeof(IGroupedEndpoint), groupType);

                var groupImpl = groupType
                    .GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IGroupedEndpoint<>))?
                    .GetGenericArguments()
                    .FirstOrDefault(x => typeof(IGroupEndpoint).IsAssignableFrom(x));

                if (groupImpl is not null && registeredGroups.Add(groupImpl))
                    services.AddSingleton(typeof(IGroupEndpoint), groupImpl);
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
            foreach (var endpoint in app.Services.GetServices<IEndpoint>())
                endpoint.AddRoute(app);

            var groupMap = app.Services.GetServices<IGroupEndpoint>()
                .GroupJoin(
                    app.Services.GetServices<IGroupedEndpoint>(),
                    parent => parent.GetType(),
                    child => child.GroupType,
                    (group, endpoints) => new { Group = group, Endpoints = endpoints }
                );

            foreach (var groupKv in groupMap)
            {
                var groupBuilder = app.MapGroup(groupKv.Group.GroupPrefix);
                groupKv.Group.Configure(groupBuilder);

                foreach (var endpoint in groupKv.Endpoints)
                    endpoint.AddRoute(groupBuilder);
            }

            return app;
        }
    }
}
