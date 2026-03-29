using System.Threading.Channels;
using CoreMesh.Outbox.Abstractions;

namespace CoreMesh.Outbox.InMemory;

/// <summary>
/// Singleton in-process channel used to pass <see cref="EventEnvelope"/> messages
/// between <see cref="InMemoryEventPublisher"/> and <see cref="InMemoryMessageSubscriber"/>.
/// </summary>
public sealed class InMemoryMessageChannel
{
    private readonly Channel<EventEnvelope> _channel =
        Channel.CreateUnbounded<EventEnvelope>(new UnboundedChannelOptions { SingleReader = true });

    /// <summary>Gets the write end of the channel.</summary>
    public ChannelWriter<EventEnvelope> Writer => _channel.Writer;

    /// <summary>Gets the read end of the channel.</summary>
    public ChannelReader<EventEnvelope> Reader => _channel.Reader;
}
