using System.Text.Json;

namespace CoreMesh.Outbox.Abstractions;

/// <summary>
/// Represents a message received from the message broker, wrapping the raw payload
/// along with routing and transport metadata.
/// </summary>
public sealed class EventEnvelope
{
    /// <summary>
    /// The event identifier extracted from the payload. Used for idempotency tracking.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// The logical event type name (matches <see cref="EventNameAttribute.EventName"/>).
    /// Used to look up the CLR type via <see cref="IEventTypeRegistry"/>.
    /// </summary>
    public string Type { get; init; } = null!;

    /// <summary>
    /// The raw JSON-serialized event payload.
    /// </summary>
    public string Payload { get; init; } = null!;

    /// <summary>
    /// The UTC timestamp when the event occurred.
    /// </summary>
    public DateTime OccurredAtUtc { get; init; }

    /// <summary>
    /// Transport-level metadata provided by the message broker (e.g. topic, partition, offset).
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers { get; init; }
        = new Dictionary<string, string>();

    /// <summary>
    /// Creates an <see cref="EventEnvelope"/> from broker message data.
    /// The <c>Id</c> is extracted from the JSON payload's <c>Id</c> field; falls back to a new <see cref="Guid"/> if absent.
    /// </summary>
    public static EventEnvelope Create(string type, string payload, DateTime occurredAtUtc, IReadOnlyDictionary<string, string> headers)
    {
        return new EventEnvelope
        {
            Id = ExtractEventId(payload),
            Type = type,
            Payload = payload,
            OccurredAtUtc = occurredAtUtc,
            Headers = headers
        };
    }

    private static Guid ExtractEventId(string payload)
    {
        using var document = JsonDocument.Parse(payload);

        if (document.RootElement.TryGetProperty("Id", out var idProperty) &&
            idProperty.ValueKind == JsonValueKind.String &&
            Guid.TryParse(idProperty.GetString(), out var id))
        {
            return id;
        }

        return Guid.NewGuid();
    }
}
