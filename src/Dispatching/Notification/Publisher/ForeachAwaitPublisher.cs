using CoreMesh.Dispatching.Abstractions;

namespace CoreMesh.Dispatching.Notification.Publisher;

/// <summary>
/// Publishes notifications to handlers sequentially, awaiting each handler before proceeding to the next.
/// </summary>
public class ForeachAwaitPublisher : INotificationPublisher
{
    /// <inheritdoc />
    public async Task Publish(IEnumerable<NotificationHandlerExecutor> handlerExecutors, INotification notification,
        CancellationToken ct)
    {
        foreach (var handlerExecutor in handlerExecutors)
        {
            await handlerExecutor.HandlerCallback(notification, ct).ConfigureAwait(false);
        }
    }
}
