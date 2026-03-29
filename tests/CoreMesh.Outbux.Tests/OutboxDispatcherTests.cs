using CoreMesh.Outbox;
using CoreMesh.Outbox.Abstractions;
using CoreMesh.Outbux.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace CoreMesh.Outbux.Tests;

public class OutboxDispatcherTests
{
    private static IServiceScopeFactory BuildScopeFactory(IOutboxStore store, IEventPublisher publisher)
        => MockScope.Build(services =>
        {
            services.AddSingleton(store);
            services.AddSingleton(publisher);
        });

    [Fact]
    public async Task PublishesClaimedMessagesAndMarksProcessed()
    {
        var message = Create.Message();
        var store = Substitute.For<IOutboxStore>();
        var publisher = Substitute.For<IEventPublisher>();

        using var cts = new CancellationTokenSource();

        store.ClaimBatchAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
             .Returns(
                 _ => (IReadOnlyList<OutboxMessage>)[message],
                 _ => { cts.Cancel(); return (IReadOnlyList<OutboxMessage>)[]; });

        var dispatcher = new OutboxDispatcher(BuildScopeFactory(store, publisher));
        await dispatcher.StartAsync(CancellationToken.None);

        try { await dispatcher.ExecuteTask!.WaitAsync(cts.Token); }
        catch (OperationCanceledException) { }

        await publisher.Received(1).PublishAsync(message, Arg.Any<CancellationToken>());
        await store.Received(1).MarkProcessedAsync(message.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task MarksFailedWhenPublishThrows()
    {
        var message = Create.Message();
        var store = Substitute.For<IOutboxStore>();
        var publisher = Substitute.For<IEventPublisher>();

        using var cts = new CancellationTokenSource();

        store.ClaimBatchAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
             .Returns(
                 _ => (IReadOnlyList<OutboxMessage>)[message],
                 _ => { cts.Cancel(); return (IReadOnlyList<OutboxMessage>)[]; });

        publisher.PublishAsync(message, Arg.Any<CancellationToken>())
                 .Returns(Task.FromException(new Exception("broker down")));

        var dispatcher = new OutboxDispatcher(BuildScopeFactory(store, publisher));
        await dispatcher.StartAsync(CancellationToken.None);

        try { await dispatcher.ExecuteTask!.WaitAsync(cts.Token); }
        catch (OperationCanceledException) { }

        await store.Received(1).MarkFailedAsync(
            message.Id,
            Arg.Any<string>(),
            Arg.Any<DateTime>(),
            Arg.Any<CancellationToken>());
        await store.DidNotReceive().MarkProcessedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CallsResetZombiesOnFirstIteration()
    {
        var store = Substitute.For<IOutboxStore>();
        var publisher = Substitute.For<IEventPublisher>();

        using var cts = new CancellationTokenSource();

        store.ClaimBatchAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
             .Returns(_ => { cts.Cancel(); return (IReadOnlyList<OutboxMessage>)[]; });

        var dispatcher = new OutboxDispatcher(BuildScopeFactory(store, publisher));
        await dispatcher.StartAsync(CancellationToken.None);

        try { await dispatcher.ExecuteTask!.WaitAsync(cts.Token); }
        catch (OperationCanceledException) { }

        await store.Received(1).ResetZombiesAsync(Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());
    }
}
