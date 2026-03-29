using CoreMesh.Outbox.InMemory;
using CoreMesh.Outbux.Tests.Helpers;

namespace CoreMesh.Outbux.Tests.InMemory;

public class InMemoryOutboxStoreConcurrencyTests
{
    [Fact]
    public async Task ConcurrentClaims_DoNotClaimSameMessage()
    {
        var store = new InMemoryOutboxStore();

        for (var i = 0; i < 10; i++)
            await store.AddAsync(new TestEvent());

        var task1 = store.ClaimBatchAsync(10, default);
        var task2 = store.ClaimBatchAsync(10, default);

        var results = await Task.WhenAll(task1, task2);

        var allClaimedIds = results.SelectMany(r => r.Select(m => m.Id)).ToList();
        var distinctIds = allClaimedIds.Distinct().ToList();

        Assert.Equal(allClaimedIds.Count, distinctIds.Count); // no duplicates
        Assert.Equal(10, distinctIds.Count);                  // all 10 messages claimed exactly once
    }

    [Fact]
    public async Task ConcurrentClaims_UnderHighContention_NoMessageClaimedTwice()
    {
        const int messageCount = 100;
        const int concurrency = 20;

        var store = new InMemoryOutboxStore();

        for (var i = 0; i < messageCount; i++)
            await store.AddAsync(new TestEvent());

        var tasks = Enumerable.Range(0, concurrency)
            .Select(_ => store.ClaimBatchAsync(10, default));

        var results = await Task.WhenAll(tasks);

        var allClaimedIds = results.SelectMany(r => r.Select(m => m.Id)).ToList();
        var distinctIds = allClaimedIds.Distinct().ToList();

        Assert.Equal(allClaimedIds.Count, distinctIds.Count); // no message claimed by two workers
        Assert.Equal(messageCount, distinctIds.Count);        // every message claimed exactly once
    }

    [Fact]
    public async Task ConcurrentAddAndClaim_DoNotCorruptState()
    {
        const int messageCount = 50;
        var store = new InMemoryOutboxStore();

        var addTasks = Enumerable.Range(0, messageCount)
            .Select(_ => store.AddAsync(new TestEvent()));

        var claimTasks = Enumerable.Range(0, 10)
            .Select(_ => store.ClaimBatchAsync(10, default));

        await Task.WhenAll(addTasks.Concat(claimTasks));

        // Drain whatever wasn't claimed yet
        var remaining = await store.ClaimBatchAsync(messageCount, default);
        var allIds = remaining.Select(m => m.Id).Distinct().ToList();

        // No exception thrown = no state corruption; all IDs are unique
        Assert.Equal(allIds.Count, remaining.Count);
    }

    [Fact]
    public async Task ConcurrentMarkProcessed_DoesNotThrow()
    {
        var store = new InMemoryOutboxStore();

        for (var i = 0; i < 20; i++)
            await store.AddAsync(new TestEvent());

        var batch = await store.ClaimBatchAsync(20, default);

        var markTasks = batch.Select(m => store.MarkProcessedAsync(m.Id, default));

        var ex = await Record.ExceptionAsync(() => Task.WhenAll(markTasks));

        Assert.Null(ex);
    }
}
