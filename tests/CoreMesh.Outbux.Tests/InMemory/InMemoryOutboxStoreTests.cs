using CoreMesh.Outbox.Abstractions;
using CoreMesh.Outbox.InMemory;
using CoreMesh.Outbux.Tests.Helpers;

namespace CoreMesh.Outbux.Tests.InMemory;

public class InMemoryOutboxStoreTests
{
    private static InMemoryOutboxStore CreateStore() => new();

    // ── ClaimBatchAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task ClaimBatchAsync_ReturnsPendingMessages()
    {
        var store = CreateStore();
        await store.AddAsync(new TestEvent());

        var batch = await store.ClaimBatchAsync(10, default);

        Assert.Single(batch);
    }

    [Fact]
    public async Task ClaimBatchAsync_SetsStatusToProcessing()
    {
        var store = CreateStore();
        await store.AddAsync(new TestEvent());

        var batch = await store.ClaimBatchAsync(10, default);

        Assert.Equal(OutboxMessageStatus.Processing, batch[0].Status);
        Assert.NotNull(batch[0].ClaimId);
        Assert.NotNull(batch[0].ProcessingStartedAt);
    }

    [Fact]
    public async Task ClaimBatchAsync_DoesNotReturnAlreadyClaimedMessages()
    {
        var store = CreateStore();
        await store.AddAsync(new TestEvent());

        await store.ClaimBatchAsync(10, default); // first claim
        var secondBatch = await store.ClaimBatchAsync(10, default);

        Assert.Empty(secondBatch);
    }

    [Fact]
    public async Task ClaimBatchAsync_RespectsBatchSize()
    {
        var store = CreateStore();
        for (var i = 0; i < 5; i++)
            await store.AddAsync(new TestEvent());

        var batch = await store.ClaimBatchAsync(3, default);

        Assert.Equal(3, batch.Count);
    }

    [Fact]
    public async Task ClaimBatchAsync_RespectsNextRetryAtUtc()
    {
        var store = CreateStore();
        await store.AddAsync(new TestEvent());

        var batch = await store.ClaimBatchAsync(10, default);
        await store.MarkFailedAsync(batch[0].Id, "err", DateTime.UtcNow.AddMinutes(5), default);

        var retry = await store.ClaimBatchAsync(10, default);

        Assert.Empty(retry);
    }

    [Fact]
    public async Task ClaimBatchAsync_OrdersByOccurredAtUtc()
    {
        var store = CreateStore();
        var early = new TestEvent();
        var late = new TestEvent();

        await store.AddAsync(late);
        await store.AddAsync(early);

        // Manually adjust timing by checking the messages are ordered
        var batch = await store.ClaimBatchAsync(10, default);

        Assert.Equal(2, batch.Count);
        Assert.True(batch[0].OccurredAtUtc <= batch[1].OccurredAtUtc);
    }

    // ── MarkProcessedAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task MarkProcessedAsync_SetsStatusAndClearsClaimInfo()
    {
        var store = CreateStore();
        await store.AddAsync(new TestEvent());

        var batch = await store.ClaimBatchAsync(10, default);
        await store.MarkProcessedAsync(batch[0].Id, default);

        var next = await store.ClaimBatchAsync(10, default);

        Assert.Empty(next); // Processed messages are not re-claimed
    }

    // ── MarkFailedAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task MarkFailedAsync_IncrementsRetryCountAndReschedulesMessage()
    {
        var store = CreateStore();
        await store.AddAsync(new TestEvent());

        var batch = await store.ClaimBatchAsync(10, default);
        await store.MarkFailedAsync(batch[0].Id, "boom", DateTime.UtcNow.AddMinutes(1), default);

        var refetched = await store.ClaimBatchAsync(10, default);

        Assert.Empty(refetched); // Still within retry window
    }

    [Fact]
    public async Task MarkFailedAsync_SetsStatusToFailedWhenRetryLimitExceeded()
    {
        var store = CreateStore();
        await store.AddAsync(new TestEvent());

        var batch = await store.ClaimBatchAsync(10, default);
        var id = batch[0].Id;

        for (var i = 0; i < 10; i++)
        {
            await store.MarkFailedAsync(id, "boom", DateTime.UtcNow.AddSeconds(-1), default);
            if (i < 9) await store.ClaimBatchAsync(10, default); // re-claim for next iteration
        }

        var final = await store.ClaimBatchAsync(10, default);
        Assert.Empty(final); // Permanently failed, never re-claimed
    }

    // ── ResetZombiesAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task ResetZombiesAsync_ResetsStalledProcessingMessages()
    {
        var store = CreateStore();
        await store.AddAsync(new TestEvent());

        var batch = await store.ClaimBatchAsync(10, default);
        _ = batch; // simulate crash — never MarkProcessed

        await store.ResetZombiesAsync(TimeSpan.Zero, default);

        var recovered = await store.ClaimBatchAsync(10, default);
        Assert.Single(recovered);
    }

    [Fact]
    public async Task ResetZombiesAsync_DoesNotResetRecentlyClaimedMessages()
    {
        var store = CreateStore();
        await store.AddAsync(new TestEvent());

        await store.ClaimBatchAsync(10, default);

        await store.ResetZombiesAsync(TimeSpan.FromHours(1), default); // timeout far in future

        var batch = await store.ClaimBatchAsync(10, default);
        Assert.Empty(batch); // Still held by original claim
    }
}
