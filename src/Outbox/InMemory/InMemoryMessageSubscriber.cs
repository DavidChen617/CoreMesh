using System.Runtime.CompilerServices;
using CoreMesh.Outbox.Abstractions;
using Microsoft.Extensions.Logging;

namespace CoreMesh.Outbox.InMemory;

/// <summary>
/// In-memory implementation of <see cref="IMessageSubscriber"/> backed by <see cref="InMemoryMessageChannel"/>.
/// <list type="bullet">
///   <item><see cref="AckAsync"/> — no-op; the message is already consumed from the channel.</item>
///   <item><see cref="NackAsync"/> — logs a warning; the message is discarded.</item>
///   <item><see cref="RetryAsync"/> — re-enqueues the envelope to the channel for reprocessing.</item>
/// </list>
/// </summary>
public sealed class InMemoryMessageSubscriber(
    InMemoryMessageChannel messageChannel,
    ILogger<InMemoryMessageSubscriber> logger) : IMessageSubscriber
{
    /// <inheritdoc/>
    public async IAsyncEnumerable<EventEnvelope> SubscribeAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var envelope in messageChannel.Reader.ReadAllAsync(cancellationToken))
        {
            yield return envelope;
        }
    }

    /// <inheritdoc/>
    public Task AckAsync(EventEnvelope envelope, CancellationToken cancellationToken)
        => Task.CompletedTask;

    /// <inheritdoc/>
    public Task NackAsync(EventEnvelope envelope, CancellationToken cancellationToken)
    {
        logger.LogWarning(
            "Event '{EventType}' (id: {EventId}) exceeded retry limit and was dead-lettered.",
            envelope.Type, envelope.Id);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task RetryAsync(EventEnvelope envelope, CancellationToken cancellationToken)
        => await messageChannel.Writer.WriteAsync(envelope, cancellationToken);
}
