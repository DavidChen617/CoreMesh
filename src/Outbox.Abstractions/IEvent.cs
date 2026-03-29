namespace CoreMesh.Outbox.Abstractions;

/// <summary>
/// Represents a domain event that can be stored in the outbox and published to a message broker.
/// </summary>
public interface IEvent
{
    /// <summary>
    /// Unique identifier of the event. Used for idempotency checks.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// The UTC timestamp when the event occurred.
    /// </summary>
    DateTime OccurredAtUtc { get; }
}
