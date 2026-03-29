namespace CoreMesh.Examples.Outbox.Outbox;

public class OutboxDispatcher(IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    private const int BatchSize = 100;
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan ZombieTimeout = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan ZombieResetInterval = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan RetryDelay = TimeSpan.FromMinutes(1);

    private DateTime _lastZombieReset = DateTime.MinValue;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var outboxStore = scope.ServiceProvider.GetRequiredService<IOutboxStore>();
            var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

            if (DateTime.UtcNow - _lastZombieReset > ZombieResetInterval)
            {
                await outboxStore.ResetZombiesAsync(ZombieTimeout, stoppingToken);
                _lastZombieReset = DateTime.UtcNow;
            }

            var messages = await outboxStore.ClaimBatchAsync(BatchSize, stoppingToken);
            if (messages.Count == 0)
            {
                await Task.Delay(PollingInterval, stoppingToken);
                continue;
            }

            foreach (var message in messages)
            {
                try
                {
                    await publisher.PublishAsync(message, stoppingToken);
                    await outboxStore.MarkProcessedAsync(message.Id, stoppingToken);
                }
                catch (Exception ex)
                {
                    await outboxStore.MarkFailedAsync(
                        message.Id,
                        ex.Message,
                        DateTime.UtcNow.Add(RetryDelay),
                        stoppingToken);
                }
            }

            if (messages.Count < BatchSize)
            {
                await Task.Delay(PollingInterval, stoppingToken);
            }
        }
    }
}