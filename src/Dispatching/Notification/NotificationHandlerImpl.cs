using Microsoft.Extensions.DependencyInjection;

namespace CoreMesh.Dispatching.Notification;

/// <summary>
/// Typed notification handler wrapper that resolves and invokes handlers for a specific notification type.
/// </summary>
/// <typeparam name="TNotification">The notification type.</typeparam>
public class NotificationHandlerImpl<TNotification> : NotificationHandler
    where TNotification : INotification
{
    /// <inheritdoc />
    public override Task Handle(INotification notification, IServiceProvider sp,
        Func<IEnumerable<NotificationHandlerExecutor>, INotification, CancellationToken, Task> publish,
        CancellationToken ct)
    {
        var handlers = sp.GetServices<INotificationHandler<TNotification>>()
            .Select(static x =>
                new NotificationHandlerExecutor(x, (notification, ct) => x.Handle((TNotification)notification, ct)));

        return publish(handlers, notification, ct);
    }
}
