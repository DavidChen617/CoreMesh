using System.Reflection;

namespace CoreMesh.Examples.Outbox.Outbox.Extensions;

public static class CoreMeshOutboxExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddCoreMeshOutbox(params Assembly[] assemblies)
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

            return services;
        }
    }
}
