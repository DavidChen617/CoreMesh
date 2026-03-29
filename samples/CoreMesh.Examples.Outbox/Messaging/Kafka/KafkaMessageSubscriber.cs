using System.Runtime.CompilerServices;
using System.Text.Json;
using Confluent.Kafka;
using CoreMesh.Examples.Outbox.Outbox;

namespace CoreMesh.Examples.Outbox.Messaging;

public class KafkaMessageSubscriber(
    IConsumer<string, string> consumer): IMessageSubscriber
{
    public async IAsyncEnumerable<EventEnvelope> SubscribeAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        consumer.Subscribe("test-todo1");

        while (!cancellationToken.IsCancellationRequested)
        {
            var cr = consumer.Consume(cancellationToken);
            var messageId = ExtractEventId(cr.Message.Value);

            yield return new EventEnvelope
            {
                Id = messageId,
                Type = cr.Message.Key,
                Payload = cr.Message.Value,
                OccurredAtUtc = cr.Message.Timestamp.UtcDateTime == default
                    ? DateTime.UtcNow
                    : cr.Message.Timestamp.UtcDateTime
            };
        }

        await Task.CompletedTask;
    }

    public Task AckAsync(EventEnvelope envelope, CancellationToken cancellationToken)
    {
        consumer.Commit();
        return Task.CompletedTask;
    }

    public Task NackAsync(EventEnvelope envelope, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private static Guid ExtractEventId(string payload)
    {
        using var document = JsonDocument.Parse(payload);

        if (document.RootElement.TryGetProperty("Id", out var idProperty) &&
            idProperty.ValueKind == JsonValueKind.String &&
            Guid.TryParse(idProperty.GetString(), out var id))
        {
            return id;
        }

        return Guid.NewGuid();
    }
}
