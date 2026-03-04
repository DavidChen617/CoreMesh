using System.Reflection;
using CoreMesh.Dispatching.Notification;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CoreMesh.Dispatching.Extensions;

/// <summary>
/// Extension methods for registering CoreMesh dispatching services and handlers.
/// </summary>
public static class DispatchingServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the dispatcher and scans assemblies for handlers with default options.
        /// </summary>
        /// <param name="assemblies">The assemblies to scan for handlers.</param>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddDispatching(params Assembly[] assemblies)
            => services.AddDispatchingCore(null, assemblies);

        /// <summary>
        /// Registers the dispatcher and scans assemblies for handlers with custom options.
        /// </summary>
        /// <param name="configure">The action to configure dispatching options.</param>
        /// <param name="assemblies">The assemblies to scan for handlers.</param>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddDispatching(Action<DispatchingOptions> configure, params Assembly[] assemblies)
            => services.AddDispatchingCore(configure, assemblies);

        private IServiceCollection AddDispatchingCore(Action<DispatchingOptions>? configure, Assembly[] assemblies)
        {
            var options = new DispatchingOptions();
            configure?.Invoke(options);

            services.AddSingleton<IDispatcher, Dispatcher>();
            services.AddSingleton(typeof(INotificationPublisher), options.NotificationPublisherType);

            var handlers = assemblies.SelectMany(x => x.DefinedTypes)
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .SelectMany(t => t.ImplementedInterfaces, (impl, iface) => new { impl, iface })
                .Where(x =>
                {
                    if (!x.iface.IsGenericType)
                        return false;
                    var genericDef = x.iface.GetGenericTypeDefinition();
                    return genericDef == typeof(IRequestHandler<,>) || genericDef == typeof(IRequestHandler<>) ||
                           genericDef == typeof(INotificationHandler<>);
                }).Select(x => new
                {
                    Interface = x.iface, Implementation = x.impl, GenericArguments = x.iface.GenericTypeArguments,
                });

            foreach (var type in handlers)
            {
                var genericDef = type.Interface.GetGenericTypeDefinition();

                if (genericDef == typeof(INotificationHandler<>))
                    services.AddTransient(type.Interface, type.Implementation);
                else
                    services.TryAddScoped(type.Interface, type.Implementation);
            }

            return services;
        }
    }
}
