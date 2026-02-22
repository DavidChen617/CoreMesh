namespace CoreMesh.Dispatching.Wrappers;

/// <summary>
/// Base wrapper abstraction for notification handler dispatching.
/// </summary>
public abstract class NotificationHandlerWrapper
{
    /// <summary>
    /// Dispatches the notification to its handlers.
    /// </summary>
    /// <param name="notification">The notification instance.</param>
    /// <param name="serviceProvider">The service provider used to resolve handlers.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when all handlers finish.</returns>
    public abstract Task Handle(
        INotification notification,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}
