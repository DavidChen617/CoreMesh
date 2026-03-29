using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Configuration;

namespace CoreMesh.Examples.Outbox.Messaging.Kafka;

public class KafkaTopicInitializer(IConfiguration configuration) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new AdminClientConfig
        {
            BootstrapServers = configuration["KafkaOption:BootstrapServers"]
        };

        using var adminClient = new AdminClientBuilder(config).Build();

        try
        {
            await adminClient.CreateTopicsAsync(
            [
                new TopicSpecification
                {
                    Name = "test-todo1",
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
