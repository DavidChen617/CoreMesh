using CoreMesh.Outbox.Abstractions;

namespace CoreMesh.Outbox.InMemory;

/// <summary>
/// In-memory implementation of both <see cref="IOutboxStore"/> and <see cref="IOutboxWriter"/>.
/// All messages are stored in process memory — suitable for development, testing, or
/// scenarios where durability across restarts is not required.
/// </summary>
public sealed class InMemoryOutboxStore : IOutboxStore, IOutboxWriter
{
    private readonly Dictionary<Guid, OutboxMessage> _messages = new();
    private readonly object _lock = new();

    /// <inheritdoc/>
    public Task AddAsync(IEvent @event, CancellationToken cancellationToken = default)
    {
        var message = OutboxMessage.Create(@event);
        lock (_lock)
        {
            _messages[message.Id] = message;
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<OutboxMessage>> ClaimBatchAsync(int batchSize, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;
            var claimId = Guid.NewGuid();

            var batch = _messages.Values
                .Where(m => m.Status == OutboxMessageStatus.Pending &&
                            (m.NextRetryAtUtc == null || m.NextRetryAtUtc <= now))
                .OrderBy(m => m.OccurredAtUtc)
                .Take(batchSize)
                .ToList();

            foreach (var msg in batch)
            {
                msg.Status = OutboxMessageStatus.Processing;
                msg.ClaimId = claimId;
                msg.ProcessingStartedAt = now;
            }

            return Task.FromResult<IReadOnlyList<OutboxMessage>>(batch);
        }
    }

    /// <inheritdoc/>
    public Task MarkProcessedAsync(Guid id, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            if (_messages.TryGetValue(id, out var message))
            {
                message.Status = OutboxMessageStatus.Processed;
                message.ProcessedAtUtc = DateTime.UtcNow;
                message.ClaimId = null;
                message.ProcessingStartedAt = null;
                message.ErrorMessage = null;
            }
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task MarkFailedAsync(Guid id, string errorMessage, DateTime nextRetryAtUtc, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            if (_messages.TryGetValue(id, out var message))
            {
                message.RetryCount++;
                message.ErrorMessage = errorMessage;
                message.NextRetryAtUtc = nextRetryAtUtc;
                message.ClaimId = null;
                message.ProcessingStartedAt = null;
                message.Status = message.RetryCount >= 10
                    ? OutboxMessageStatus.Failed
                    : OutboxMessageStatus.Pending;
            }
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task ResetZombiesAsync(TimeSpan processingTimeout, CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow - processingTimeout;

        lock (_lock)
        {
            foreach (var message in _messages.Values
                         .Where(m => m.Status == OutboxMessageStatus.Processing &&
                                     m.ProcessingStartedAt <= cutoff))
            {
                message.Status = OutboxMessageStatus.Pending;
                message.ClaimId = null;
                message.ProcessingStartedAt = null;
            }
        }

        return Task.CompletedTask;
    }
}
