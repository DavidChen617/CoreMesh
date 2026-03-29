using CoreMesh.Examples.Outbox.Outbox;

namespace CoreMesh.Examples.Outbox.Messaging;

public class EventDispatcher(IEnumerable<IEventHandler> handlers) : IEventDispatcher
{
    private readonly IReadOnlyDictionary<string, IEventHandler> _handlers =
        handlers.ToDictionary(x => x.EventType, StringComparer.OrdinalIgnoreCase);

    public Task DispatchAsync(EventEnvelope envelope, CancellationToken cancellationToken)
    {
        if (!_handlers.TryGetValue(envelope.Type, out var handler) || handler is null)
        {
            throw new InvalidOperationException($"No handler registered for event type '{envelope.Type}'.");
        }

        return handler.HandleAsync(envelope, cancellationToken);
    }
}
