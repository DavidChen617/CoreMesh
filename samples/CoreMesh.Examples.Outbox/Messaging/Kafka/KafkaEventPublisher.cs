using Confluent.Kafka;
using CoreMesh.Examples.Outbox.Outbox;

namespace CoreMesh.Examples.Outbox.Messaging;

public sealed class KafkaEventPublisher(IProducer<string, string> producer) : IEventPublisher
{
    public async Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        await producer.ProduceAsync(
            "test-todo1",
            new Message<string, string>
            {
                Key = message.EventType,
                Value = message.Payload,
                Timestamp = new Timestamp(message.OccurredAtUtc)
            },
            cancellationToken);
    }
}
