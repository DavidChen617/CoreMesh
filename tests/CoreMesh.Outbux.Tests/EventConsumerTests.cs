using CoreMesh.Outbox;
using CoreMesh.Outbox.Abstractions;
using CoreMesh.Outbux.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace CoreMesh.Outbux.Tests;

public class EventConsumerTests
{
    private static (EventConsumer Consumer, FakeMessageSubscriber Subscriber) Build(
        IEventDispatcher dispatcher,
        params EventEnvelope[] envelopes)
    {
        var subscriber = new FakeMessageSubscriber(envelopes);

        var scopeFactory = MockScope.Build(services =>
            services.AddSingleton(dispatcher));

        var consumer = new EventConsumer(
            subscriber,
            scopeFactory,
            MockScope.NullLogger<EventConsumer>());

        return (consumer, subscriber);
    }

    [Fact]
    public async Task AcksEnvelopeOnSuccessfulDispatch()
    {
        var envelope = Create.Envelope();
        var dispatcher = Substitute.For<IEventDispatcher>();

        var (consumer, subscriber) = Build(dispatcher, envelope);

        await consumer.StartAsync(CancellationToken.None);
        await consumer.ExecuteTask!;

        Assert.Contains(envelope, subscriber.Acked);
        Assert.Empty(subscriber.Nacked);
    }

    [Fact]
    public async Task RetriesEnvelopeOnDispatchFailure()
    {
        var envelope = Create.Envelope();
        var dispatcher = Substitute.For<IEventDispatcher>();

        dispatcher.DispatchAsync(envelope, Arg.Any<CancellationToken>())
                  .Returns(Task.FromException(new Exception("dispatch failed")));

        var (consumer, subscriber) = Build(dispatcher, envelope);

        await consumer.StartAsync(CancellationToken.None);
        await consumer.ExecuteTask!;

        Assert.Contains(envelope, subscriber.Retried);
        Assert.Empty(subscriber.Acked);
        Assert.Empty(subscriber.Nacked);
    }

    [Fact]
    public async Task NacksEnvelopeWhenMaxRetriesExceeded()
    {
        // FakeMessageSubscriber yields the same envelope 3 times to simulate retry re-delivery
        var envelope = Create.Envelope();
        var dispatcher = Substitute.For<IEventDispatcher>();

        dispatcher.DispatchAsync(envelope, Arg.Any<CancellationToken>())
                  .Returns(Task.FromException(new Exception("always fails")));

        var (consumer, subscriber) = Build(dispatcher, envelope, envelope, envelope);

        await consumer.StartAsync(CancellationToken.None);
        await consumer.ExecuteTask!;

        Assert.Contains(envelope, subscriber.Nacked);
        Assert.Empty(subscriber.Acked);
    }

    [Fact]
    public async Task ResetsRetryCountAfterSuccessfulDispatch()
    {
        var envelope = Create.Envelope();
        var dispatcher = Substitute.For<IEventDispatcher>();

        var callCount = 0;
        dispatcher.DispatchAsync(envelope, Arg.Any<CancellationToken>())
                  .Returns(_ =>
                  {
                      callCount++;
                      // Fail first 2 times, succeed on 3rd
                      return callCount < 3
                          ? Task.FromException(new Exception("fail"))
                          : Task.CompletedTask;
                  });

        // envelope appears 3 times: 2 retries + 1 success
        var (consumer, subscriber) = Build(dispatcher, envelope, envelope, envelope);

        await consumer.StartAsync(CancellationToken.None);
        await consumer.ExecuteTask!;

        Assert.Contains(envelope, subscriber.Acked);
        Assert.Empty(subscriber.Nacked);
    }
}
