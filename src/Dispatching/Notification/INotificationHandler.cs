namespace CoreMesh.Dispatching;

/// <summary>
/// Handles a notification of the specified type.
/// </summary>
/// <typeparam name="TNotification">The notification type.</typeparam>
public interface INotificationHandler<in TNotification> where TNotification : INotification
{
    /// <summary>
    /// Handles the specified notification.
    /// </summary>
    /// <param name="notification">The notification instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task that completes when handling finishes.</returns>
    Task Handle(TNotification notification, CancellationToken ct);
}
