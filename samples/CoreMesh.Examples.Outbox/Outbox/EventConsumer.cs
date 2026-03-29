using CoreMesh.Examples.Outbox.Outbox;

namespace CoreMesh.Examples.Outbox.Messaging;

public class EventConsumer( 
    IMessageSubscriber subscriber,
    IEventDispatcher dispatcher): BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var envelope in subscriber.SubscribeAsync(stoppingToken))
        {
            try
            {
                await dispatcher.DispatchAsync(envelope, stoppingToken);
                await subscriber.AckAsync(envelope, stoppingToken);
            }
            catch
            {
                await subscriber.NackAsync(envelope, stoppingToken);
            }
        }
    }
}
