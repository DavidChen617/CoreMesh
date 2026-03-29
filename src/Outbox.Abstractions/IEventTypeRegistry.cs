namespace CoreMesh.Outbox.Abstractions;

/// <summary>
/// Maps event type name strings to their corresponding CLR types.
/// Used by <see cref="IEventDispatcher"/> to deserialize incoming payloads.
/// </summary>
public interface IEventTypeRegistry
{
    /// <summary>
    /// Returns the CLR <see cref="Type"/> registered for the given <paramref name="eventType"/> name.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no type is registered for <paramref name="eventType"/>.
    /// </exception>
    Type GetEventClrType(string eventType);
}
