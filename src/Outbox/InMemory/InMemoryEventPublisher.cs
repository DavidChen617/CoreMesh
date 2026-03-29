using CoreMesh.Outbox.Abstractions;

namespace CoreMesh.Outbox.InMemory;

/// <summary>
/// In-memory implementation of <see cref="IEventPublisher"/> that writes published messages
/// to an in-process <see cref="InMemoryMessageChannel"/> for consumption by <see cref="InMemoryMessageSubscriber"/>.
/// </summary>
public sealed class InMemoryEventPublisher(InMemoryMessageChannel messageChannel) : IEventPublisher
{
    /// <inheritdoc/>
    public async Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        var envelope = EventEnvelope.Create(
            message.EventType,
            message.Payload,
            message.OccurredAtUtc,
            new Dictionary<string, string>());

        await messageChannel.Writer.WriteAsync(envelope, cancellationToken);
    }
}
