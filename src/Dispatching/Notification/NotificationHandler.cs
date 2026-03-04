namespace CoreMesh.Dispatching.Notification;

/// <summary>
/// Base class for notification handler wrappers used internally by the dispatcher.
/// </summary>
public abstract class NotificationHandler
{
    /// <summary>
    /// Handles the notification by resolving handlers and delegating to the publish function.
    /// </summary>
    /// <param name="notification">The notification instance.</param>
    /// <param name="sp">The service provider.</param>
    /// <param name="publish">The publish delegate.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task that completes when all handlers finish.</returns>
    public abstract Task Handle(INotification notification, IServiceProvider sp,
        Func<IEnumerable<NotificationHandlerExecutor>, INotification, CancellationToken, Task> publish,
        CancellationToken ct);
}
