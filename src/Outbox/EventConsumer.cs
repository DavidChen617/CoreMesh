using CoreMesh.Outbox.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CoreMesh.Outbox;

/// <summary>
/// Background service that consumes messages from <see cref="IMessageSubscriber"/>
/// and dispatches each one to its registered <see cref="IEventHandler{TEvent}"/>.
/// </summary>
/// <remarks>
/// Failed dispatches are retried up to <c>MaxRetries</c> times via <see cref="IMessageSubscriber.RetryAsync"/>.
/// After all retries are exhausted the message is permanently rejected via <see cref="IMessageSubscriber.NackAsync"/>.
/// </remarks>
public class EventConsumer(
    IMessageSubscriber subscriber,
    IServiceScopeFactory scopeFactory,
    ILogger<EventConsumer> logger
) : BackgroundService
{
    private const int MaxRetries = 3;
    private readonly Dictionary<Guid, int> _retryCounts = new();

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var envelope in subscriber.SubscribeAsync(stoppingToken))
        {
            try
            {
                using var messageScope = scopeFactory.CreateScope();
                var dispatcher = messageScope.ServiceProvider.GetRequiredService<IEventDispatcher>();
                await dispatcher.DispatchAsync(envelope, stoppingToken);
                _retryCounts.Remove(envelope.Id);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                var attempt = _retryCounts.GetValueOrDefault(envelope.Id, 0) + 1;

                logger.LogError(ex,
                    "Failed to consume event '{EventType}' (id: {EventId}, attempt: {Attempt}/{Max})",
                    envelope.Type, envelope.Id, attempt, MaxRetries);

                if (attempt >= MaxRetries)
                {
                    _retryCounts.Remove(envelope.Id);
                    await subscriber.NackAsync(envelope, stoppingToken);
                }
                else
                {
                    _retryCounts[envelope.Id] = attempt;
                    await subscriber.RetryAsync(envelope, stoppingToken);
                }

                continue;
            }

            await subscriber.AckAsync(envelope, stoppingToken);
        }
    }
}
