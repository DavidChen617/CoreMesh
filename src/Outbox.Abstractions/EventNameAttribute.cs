namespace CoreMesh.Outbox.Abstractions;

/// <summary>
/// Specifies the logical event type name used when serializing to and deserializing from the outbox.
/// Must be applied to every class that implements <see cref="IEvent"/>.
/// </summary>
/// <example>
/// <code>
/// [EventName("todo.created")]
/// public record TodoCreatedEvent : IEvent { ... }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class EventNameAttribute(string eventName) : Attribute
{
    /// <summary>
    /// The logical event type name stored in the outbox and published to the message broker.
    /// </summary>
    public string EventName => eventName;
}
