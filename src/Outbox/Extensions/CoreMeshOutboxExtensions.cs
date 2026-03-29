using System.Reflection;
using CoreMesh.Outbox.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace CoreMesh.Outbox.Extensions;

/// <summary>
/// Extension methods for registering CoreMesh Outbox services.
/// </summary>
public static class CoreMeshOutboxExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the core outbox services and applies the given <paramref name="configure"/> delegate.
        /// <list type="bullet">
        ///   <item><see cref="IEventTypeRegistry"/> — populated by scanning <paramref name="assemblies"/>.</item>
        ///   <item><see cref="IEventHandler{TEvent}"/> implementations found in <paramref name="assemblies"/>.</item>
        ///   <item><see cref="IEventDispatcher"/> → <see cref="EventDispatcher"/>.</item>
        ///   <item><see cref="OutboxDispatcher"/> as a hosted background service.</item>
        /// </list>
        /// Use <paramref name="configure"/> to set up storage and transport:
        /// <code>
        /// services.AddCoreMeshOutbox(
        ///     [typeof(Program).Assembly],
        ///     options => {
        ///         options.UseInMemoryStore();
        ///         options.UseInMemoryChannel();
        ///         options.WithConsumer();
        ///     });
        /// </code>
        /// </summary>
        public IServiceCollection AddCoreMeshOutbox(
            Assembly[] assemblies,
            Action<CoreMeshOutboxOptions> configure)
        {
            var allTypes = assemblies.SelectMany(x => x.GetTypes()).ToList();

            var eventTypes = allTypes.Where(t =>
                typeof(IEvent).IsAssignableFrom(t) &&
                t is { IsAbstract: false, IsInterface: false });

            var registerEvent = new Dictionary<string, Type>();

            foreach (var type in eventTypes)
            {
                var handlerInterfaceType = typeof(IEventHandler<>).MakeGenericType(type);
                var handlerImplementationType = allTypes.FirstOrDefault(t =>
                    !t.IsAbstract &&
                    !t.IsInterface &&
                    handlerInterfaceType.IsAssignableFrom(t));

                if (handlerImplementationType is null)
                {
                    continue;
                }

                var eventNameAttribute = type.GetCustomAttribute<EventNameAttribute>()
                                         ?? throw new InvalidOperationException(
                                             $"Event '{type.FullName}' is missing EventNameAttribute.");

                registerEvent.Add(eventNameAttribute.EventName, type);
                services.AddScoped(handlerInterfaceType, handlerImplementationType);
            }

            services.AddSingleton<IEventTypeRegistry>(_ => new EventTypeRegistry(registerEvent));
            services.AddScoped<IEventDispatcher, EventDispatcher>();
            services.AddHostedService<OutboxDispatcher>();

            configure(new CoreMeshOutboxOptions(services));

            return services;
        }
    }
}
