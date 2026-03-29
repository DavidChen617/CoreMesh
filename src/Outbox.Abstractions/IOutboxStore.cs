namespace CoreMesh.Outbox.Abstractions;

/// <summary>
/// Provides persistence operations for the outbox message store.
/// </summary>
public interface IOutboxStore
{
    /// <summary>
    /// Atomically claims up to <paramref name="batchSize"/> pending messages for processing.
    /// Only messages with <c>Status = Pending</c> and a due <c>NextRetryAtUtc</c> are eligible.
    /// Returns only the messages successfully claimed by this call.
    /// </summary>
    Task<IReadOnlyList<OutboxMessage>> ClaimBatchAsync(int batchSize, CancellationToken cancellationToken);

    /// <summary>
    /// Marks the message with the given <paramref name="id"/> as successfully processed.
    /// </summary>
    Task MarkProcessedAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Records a processing failure for the message with the given <paramref name="id"/>.
    /// The message is rescheduled for retry at <paramref name="nextRetryAtUtc"/>.
    /// If the retry limit is exceeded, the message is permanently marked as <c>Failed</c>.
    /// </summary>
    Task MarkFailedAsync(Guid id, string errorMessage, DateTime nextRetryAtUtc, CancellationToken cancellationToken);

    /// <summary>
    /// Resets messages stuck in <c>Processing</c> status back to <c>Pending</c>
    /// if their <c>ProcessingStartedAt</c> exceeds <paramref name="processingTimeout"/>.
    /// This recovers messages from crashed or stalled dispatcher instances.
    /// </summary>
    Task ResetZombiesAsync(TimeSpan processingTimeout, CancellationToken cancellationToken);
}
