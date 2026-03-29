using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace CoreMesh.Examples.Outbox.Messaging;

public class KafkaTopicInitializer : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new AdminClientConfig { BootstrapServers = "192.168.65.4:9092" };

        using var adminClient = new AdminClientBuilder(config).Build();

        try
        {
            await adminClient.CreateTopicsAsync(
            [
                new TopicSpecification { Name = "test-todo1", 
                    NumPartitions = 1,
                    ReplicationFactor = 1 
                }
            ]);
        }
        catch (CreateTopicsException ex)
            when (ex.Results.All(x => x.Error.Code == ErrorCode.TopicAlreadyExists))
        {
        }
    }
}
