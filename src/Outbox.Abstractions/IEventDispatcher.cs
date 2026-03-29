namespace CoreMesh.Outbox.Abstractions;

/// <summary>
/// Resolves and invokes the appropriate <see cref="IEventHandler{TEvent}"/> for a received <see cref="EventEnvelope"/>.
/// </summary>
public interface IEventDispatcher
{
    /// <summary>
    /// Deserializes the payload from <paramref name="envelope"/> and dispatches it to its registered handler.
    /// </summary>
    Task DispatchAsync(EventEnvelope envelope, CancellationToken cancellationToken);
}
