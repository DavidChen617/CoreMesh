using System.Reflection;
using System.Text.Json;

namespace CoreMesh.Outbox.Abstractions;

/// <summary>
/// Represents a persisted outbox entry. Created from an <see cref="IEvent"/> and stored
/// atomically alongside the domain entity change, then published to the broker by <see cref="IEventPublisher"/>.
/// </summary>
public sealed class OutboxMessage
{
    /// <summary>
    /// Unique identifier of the message, sourced from <see cref="IEvent.Id"/>.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// The logical event type name (from <see cref="EventNameAttribute"/>).
    /// Used as the routing key when publishing to the broker.
    /// </summary>
    public string EventType { get; set; } = null!;

    /// <summary>
    /// JSON-serialized event payload.
    /// </summary>
    public string Payload { get; set; } = null!;

    /// <summary>
    /// The UTC timestamp when the event occurred.
    /// </summary>
    public DateTime OccurredAtUtc { get; set; }

    /// <summary>
    /// The UTC timestamp when the message was successfully published. <c>null</c> if not yet processed.
    /// </summary>
    public DateTime? ProcessedAtUtc { get; set; }

    /// <summary>
    /// The earliest UTC time at which this message should next be retried.
    /// <c>null</c> means it is immediately eligible for pickup.
    /// </summary>
    public DateTime? NextRetryAtUtc { get; set; }

    /// <summary>
    /// The number of failed publish attempts so far.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Current lifecycle status of the message.
    /// </summary>
    public OutboxMessageStatus Status { get; set; } = OutboxMessageStatus.Pending;

    /// <summary>
    /// The error message from the last failed publish attempt. <c>null</c> when not failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Identifies which dispatcher instance currently holds this message.
    /// Set during <c>ClaimBatchAsync</c> and cleared on completion or failure.
    /// Used to detect zombie messages.
    /// </summary>
    public Guid? ClaimId { get; set; }

    /// <summary>
    /// The UTC time when this message was claimed for processing.
    /// Used alongside <c>ClaimId</c> to detect and recover zombie messages.
    /// </summary>
    public DateTime? ProcessingStartedAt { get; set; }

    private OutboxMessage() { }

    /// <summary>
    /// Creates an <see cref="OutboxMessage"/> from the given <paramref name="event"/>.
    /// The event type must be decorated with <see cref="EventNameAttribute"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the event type is missing <see cref="EventNameAttribute"/>.
    /// </exception>
    public static OutboxMessage Create(IEvent @event)
    {
        var eventType = @event.GetType();
        var eventName = eventType.GetCustomAttribute<EventNameAttribute>()?.EventName
                        ?? throw new InvalidOperationException(
                            $"Event '{eventType.FullName}' is missing EventNameAttribute.");

        return new OutboxMessage
        {
            Id = @event.Id,
            EventType = eventName,
            Payload = JsonSerializer.Serialize(@event, @event.GetType()),
            OccurredAtUtc = @event.OccurredAtUtc,
            Status = OutboxMessageStatus.Pending,
            NextRetryAtUtc = null
        };
    }
}

/// <summary>
/// Lifecycle status of an <see cref="OutboxMessage"/>.
/// </summary>
public enum OutboxMessageStatus
{
    /// <summary>Message is waiting to be claimed and published.</summary>
    Pending = 0,

    /// <summary>Message was successfully published to the broker.</summary>
    Processed = 1,

    /// <summary>Message exceeded the retry limit and will not be retried.</summary>
    Failed = 2,

    /// <summary>Message has been claimed by a dispatcher instance and is currently being published.</summary>
    Processing = 3
}
