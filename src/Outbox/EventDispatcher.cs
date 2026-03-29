using System.Text.Json;

namespace CoreMesh.Examples.Outbox.Outbox;

public class EventDispatcher(
    IServiceProvider serviceProvider,
    IEventTypeRegistry eventTypeRegistry) : IEventDispatcher
{
    public async Task DispatchAsync(EventEnvelope envelope, CancellationToken cancellationToken)
    {
        var eventClrType = eventTypeRegistry.GetEventClrType(envelope.Type);
        var @event = JsonSerializer.Deserialize(envelope.Payload, eventClrType) as IEvent
                     ?? throw new InvalidOperationException($"Invalid payload for event type '{envelope.Type}'.");

        var handlerType = typeof(IEventHandler<>).MakeGenericType(eventClrType);
        var handler = (IEventHandler)serviceProvider.GetRequiredService(handlerType);

        await handler.HandleAsync(@event, cancellationToken);
    }
}
