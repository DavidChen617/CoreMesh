using CoreMesh.Examples.Outbox.Data;
using CoreMesh.Examples.Outbox.Outbox;
using Microsoft.EntityFrameworkCore;

namespace CoreMesh.Examples.Outbox.Messaging;

public class OutboxDispatcher(IServiceScopeFactory serviceScopeFactory): BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
            var now = DateTime.UtcNow;

            var query = dbContext.OutboxMessages
                .Where(x => x.Status == OutboxMessageStatus.Pending)
                .Where(x => x.NextRetryAtUtc == null || x.NextRetryAtUtc <= now);

            var messages = await query
                .OrderBy(x => x.OccurredAtUtc)
                .Take(100)
                .ToListAsync(stoppingToken);

            if (messages.Count == 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                continue;
            }

            foreach (var message in messages)
            {
                try
                {
                    await publisher.PublishAsync(message, stoppingToken);

                    message.Status = OutboxMessageStatus.Processed;
                    message.ProcessedAtUtc = DateTime.UtcNow;
                    message.ErrorMessage = null;
                }
                catch (Exception ex)
                {
                    message.RetryCount++;
                    message.ErrorMessage = ex.Message;
                    message.NextRetryAtUtc = DateTime.UtcNow.AddMinutes(1);

                    if (message.RetryCount >= 10)
                    {
                        message.Status = OutboxMessageStatus.Failed;
                    }
                }
            }

            await dbContext.SaveChangesAsync(stoppingToken);
        }
    }
}
