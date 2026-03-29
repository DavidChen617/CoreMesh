namespace CoreMesh.Outbox.Abstractions;

/// <summary>
/// Subscribes to incoming messages from a message broker and controls offset/acknowledgement lifecycle.
/// </summary>
public interface IMessageSubscriber
{
    /// <summary>
    /// Returns an async stream of incoming <see cref="EventEnvelope"/> messages.
    /// The stream continues until <paramref name="cancellationToken"/> is cancelled.
    /// </summary>
    IAsyncEnumerable<EventEnvelope> SubscribeAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Acknowledges successful processing of <paramref name="envelope"/>.
    /// Commits the broker offset so the message is not re-delivered.
    /// </summary>
    Task AckAsync(EventEnvelope envelope, CancellationToken cancellationToken);

    /// <summary>
    /// Permanently rejects <paramref name="envelope"/> after all retry attempts are exhausted.
    /// The message is forwarded to a dead-letter destination and the broker offset is committed.
    /// </summary>
    Task NackAsync(EventEnvelope envelope, CancellationToken cancellationToken);

    /// <summary>
    /// Signals that <paramref name="envelope"/> should be retried.
    /// The broker offset is seeked back so the message will be re-delivered on the next consume.
    /// </summary>
    Task RetryAsync(EventEnvelope envelope, CancellationToken cancellationToken);
}
