using System.Text.Json;
using CoreMesh.Outbox.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace CoreMesh.Outbox;

/// <summary>
/// Default implementation of <see cref="IEventDispatcher"/>.
/// Deserializes the payload from the <see cref="EventEnvelope"/> using <see cref="IEventTypeRegistry"/>,
/// then resolves and invokes the matching <see cref="IEventHandler{TEvent}"/> from the DI container.
/// </summary>
public class EventDispatcher(
    IServiceProvider serviceProvider,
    IEventTypeRegistry eventTypeRegistry) : IEventDispatcher
{
    /// <inheritdoc/>
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
