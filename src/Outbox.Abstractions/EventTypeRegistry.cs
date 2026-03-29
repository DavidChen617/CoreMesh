namespace CoreMesh.Outbox.Abstractions;

/// <summary>
/// Default implementation of <see cref="IEventTypeRegistry"/> backed by an immutable dictionary.
/// Populated at startup via <c>AddCoreMeshOutbox</c> by scanning registered assemblies.
/// </summary>
public sealed class EventTypeRegistry(IReadOnlyDictionary<string, Type> eventTypes) : IEventTypeRegistry
{
    /// <inheritdoc/>
    public Type GetEventClrType(string eventType)
    {
        if (!eventTypes.TryGetValue(eventType, out var eventClrType))
        {
            throw new InvalidOperationException($"No event CLR type registered for '{eventType}'.");
        }

        return eventClrType;
    }
}
