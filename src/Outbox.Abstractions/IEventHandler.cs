namespace CoreMesh.Outbox.Abstractions;

/// <summary>
/// Non-generic base interface for event handlers. Used internally for type-erased dispatch.
/// </summary>
public interface IEventHandler
{
    /// <summary>
    /// Handles the given event.
    /// </summary>
    Task HandleAsync(IEvent @event, CancellationToken cancellationToken);
}

/// <summary>
/// Strongly-typed event handler for <typeparamref name="TEvent"/>.
/// Implement this interface to handle a specific event type.
/// </summary>
/// <typeparam name="TEvent">The event type this handler processes.</typeparam>
public interface IEventHandler<in TEvent> : IEventHandler
    where TEvent : IEvent
{
    /// <summary>
    /// Handles the given <typeparamref name="TEvent"/>.
    /// </summary>
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken);

    async Task IEventHandler.HandleAsync(IEvent @event, CancellationToken cancellationToken)
    {
        if (@event is not TEvent typedEvent)
        {
            throw new InvalidOperationException(
                $"Invalid event type '{@event.GetType().FullName}' for handler '{GetType().FullName}'.");
        }

        await HandleAsync(typedEvent, cancellationToken);
    }
}
