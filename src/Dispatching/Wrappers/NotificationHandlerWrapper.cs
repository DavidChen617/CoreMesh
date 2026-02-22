namespace CoreMesh.Dispatching.Wrappers;

public abstract class NotificationHandlerWrapper
{
    public abstract Task Handle(
        INotification notification,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}
