namespace CoreMesh.Outbox.Abstractions;

/// <summary>
/// Publishes an <see cref="OutboxMessage"/> to the underlying message broker (e.g. Kafka, RabbitMQ).
/// Implement this interface to provide a broker-specific transport.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes the given <paramref name="message"/> to the message broker.
    /// </summary>
    Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken = default);
}
