using CoreMesh.Examples.Outbox.Data;
using CoreMesh.Outbox.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace CoreMesh.Examples.Outbox.Messaging;

public class EfCoreOutboxStore(AppDbContext db) : IOutboxStore
{
    public async Task<IReadOnlyList<OutboxMessage>> ClaimBatchAsync(int batchSize, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var claimId = Guid.NewGuid();

        var candidateIds = await db.OutboxMessages
            .Where(x => x.Status == OutboxMessageStatus.Pending &&
                        (x.NextRetryAtUtc == null || x.NextRetryAtUtc <= now))
            .OrderBy(x => x.OccurredAtUtc)
            .Take(batchSize)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (candidateIds.Count == 0)
            return [];

        await db.OutboxMessages
            .Where(x => candidateIds.Contains(x.Id) && x.Status == OutboxMessageStatus.Pending)
            .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, OutboxMessageStatus.Processing)
                    .SetProperty(x => x.ClaimId, claimId)
                    .SetProperty(x => x.ProcessingStartedAt, now),
                cancellationToken);

        return await db.OutboxMessages
            .Where(x => x.ClaimId == claimId)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkProcessedAsync(Guid id, CancellationToken cancellationToken)
    {
        await db.OutboxMessages
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, OutboxMessageStatus.Processed)
                    .SetProperty(x => x.ProcessedAtUtc, DateTime.UtcNow)
                    .SetProperty(x => x.ClaimId, (Guid?)null)
                    .SetProperty(x => x.ProcessingStartedAt, (DateTime?)null)
                    .SetProperty(x => x.ErrorMessage, (string?)null),
                cancellationToken);
    }

    public async Task MarkFailedAsync(
        Guid id,
        string errorMessage,
        DateTime nextRetryAtUtc,
        CancellationToken cancellationToken)
    {
        var message = await db.OutboxMessages.FirstAsync(x => x.Id == id, cancellationToken);

        message.RetryCount++;
        message.ErrorMessage = errorMessage;
        message.NextRetryAtUtc = nextRetryAtUtc;
        message.ClaimId = null;
        message.ProcessingStartedAt = null;
        message.Status = message.RetryCount >= 10
            ? OutboxMessageStatus.Failed
            : OutboxMessageStatus.Pending;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ResetZombiesAsync(TimeSpan processingTimeout, CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow - processingTimeout;

        await db.OutboxMessages
            .Where(x => x.Status == OutboxMessageStatus.Processing && x.ProcessingStartedAt <= cutoff)
            .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, OutboxMessageStatus.Pending)
                    .SetProperty(x => x.ClaimId, (Guid?)null)
                    .SetProperty(x => x.ProcessingStartedAt, (DateTime?)null),
                cancellationToken);
    }
}
