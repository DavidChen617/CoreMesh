using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CoreMesh.Dispatching;

/// <summary>
/// Extension methods for registering CoreMesh dispatching services and handlers.
/// </summary>
public static class DispatchingServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the dispatcher and scans all currently loaded assemblies for handlers.
        /// </summary>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddDispatching()
        {
            services.TryAddScoped<IDispatcher, Dispatcher>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                RegisterHandlerFromAssembly(services, assembly);

            return services;
        }

        /// <summary>
        /// Registers the dispatcher and scans the specified assemblies for handlers.
        /// </summary>
        /// <param name="assemblies">The assemblies to scan.</param>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddDispatching(params Assembly[] assemblies)
        {
            services.TryAddScoped<IDispatcher, Dispatcher>();

            foreach (var assembly in assemblies)
                RegisterHandlerFromAssembly(services, assembly);

            return services;
        }
    }

    private static void RegisterHandlerFromAssembly(IServiceCollection services, Assembly assembly)
    {
        var implementationTypes = assembly.DefinedTypes
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .ToArray();

        foreach (var implementationType in implementationTypes)
        {
            var serviceTypes = implementationType.ImplementedInterfaces
                .Where(IsDispatchingHandlerInterface)
                .ToArray();

            foreach (var serviceType in serviceTypes)
                services.TryAddEnumerable(new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Scoped));
        }
    }

    private static bool IsDispatchingHandlerInterface(Type type)
    {
        if (!type.IsGenericType)
            return false;

        var genericType = type.GetGenericTypeDefinition();

        return genericType == typeof(IRequestHandler<>)
               || genericType == typeof(IRequestHandler<,>)
               || genericType == typeof(INotificationHandler<>);
    }
}
