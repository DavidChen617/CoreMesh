using System.Runtime.CompilerServices;
using CoreMesh.Outbox.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CoreMesh.Outbux.Tests.Helpers;

[EventName("test.event")]
public sealed record TestEvent : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}

public static class Create
{
    public static OutboxMessage Message() => OutboxMessage.Create(new TestEvent());

    public static EventEnvelope Envelope(string type = "test.event")
    {
        var id = Guid.NewGuid();
        var payload = $$"""{"Id":"{{id}}","OccurredAtUtc":"{{DateTime.UtcNow:O}}"}""";
        return EventEnvelope.Create(type, payload, DateTime.UtcNow, new Dictionary<string, string>());
    }
}

/// <summary>
/// Fake IMessageSubscriber that yields a fixed set of envelopes, then completes.
/// </summary>
public sealed class FakeMessageSubscriber(params EventEnvelope[] envelopes) : IMessageSubscriber
{
    public List<EventEnvelope> Acked { get; } = [];
    public List<EventEnvelope> Nacked { get; } = [];
    public List<EventEnvelope> Retried { get; } = [];

    public async IAsyncEnumerable<EventEnvelope> SubscribeAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var envelope in envelopes)
        {
            yield return envelope;
        }

        await Task.CompletedTask;
    }

    public Task AckAsync(EventEnvelope envelope, CancellationToken cancellationToken)
    {
        Acked.Add(envelope);
        return Task.CompletedTask;
    }

    public Task NackAsync(EventEnvelope envelope, CancellationToken cancellationToken)
    {
        Nacked.Add(envelope);
        return Task.CompletedTask;
    }

    public Task RetryAsync(EventEnvelope envelope, CancellationToken cancellationToken)
    {
        Retried.Add(envelope);
        return Task.CompletedTask;
    }
}

public static class MockScope
{
    /// <summary>
    /// Builds a mock IServiceScopeFactory that resolves the given services.
    /// </summary>
    public static IServiceScopeFactory Build(Action<IServiceCollection> register)
    {
        var services = new ServiceCollection();
        register(services);
        var provider = services.BuildServiceProvider();

        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(provider);

        var factory = Substitute.For<IServiceScopeFactory>();
        factory.CreateScope().Returns(scope);

        return factory;
    }

    public static ILogger<T> NullLogger<T>() => Microsoft.Extensions.Logging.Abstractions.NullLogger<T>.Instance;
}
