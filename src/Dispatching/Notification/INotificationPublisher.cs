namespace CoreMesh.Dispatching.Notification;

/// <summary>
/// Defines the strategy for publishing notifications to multiple handlers.
/// </summary>
public interface INotificationPublisher
{
    /// <summary>
    /// Publishes a notification to all provided handler executors.
    /// </summary>
    /// <param name="handlerExecutors">The handler executors to invoke.</param>
    /// <param name="notification">The notification instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task that completes when all handlers finish.</returns>
    Task Publish(IEnumerable<NotificationHandlerExecutor> handlerExecutors, INotification notification,
        CancellationToken ct);
}
