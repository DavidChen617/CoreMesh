using Microsoft.Extensions.DependencyInjection;

namespace CoreMesh.Dispatching.Wrappers;

public class NotificationHandlerWrapperImpl<TNotification> : NotificationHandlerWrapper
    where TNotification : INotification
{
    public override async Task Handle(INotification notification, IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var typedNotification = (TNotification)notification;
        var resolved = serviceProvider.GetServices<INotificationHandler<TNotification>>();
        var handlers = resolved as INotificationHandler<TNotification>[] ?? resolved.ToArray();

        switch (handlers.Length)
        {
            case 0:
                return;

            case 1:
                await handlers[0].Handle(typedNotification, cancellationToken).ConfigureAwait(false);
                return;
        }

        foreach (var t in handlers)
            await t.Handle(typedNotification, cancellationToken).ConfigureAwait(false);
    }
}
