using CoreMesh.Dispatching.Abstractions;

namespace CoreMesh.Dispatching.Notification.Publisher;

/// <summary>
/// Publishes notifications to all handlers in parallel using <see cref="Task.WhenAll(IEnumerable{Task})"/>.
/// If multiple handlers throw exceptions, an <see cref="AggregateException"/> is thrown.
/// </summary>
public class TaskWhenAllPublisher : INotificationPublisher
{
    /// <inheritdoc />
    public async Task Publish(IEnumerable<NotificationHandlerExecutor> handlerExecutors, INotification notification,
        CancellationToken ct)
    {
        var tasks = handlerExecutors.Select(x => x.HandlerCallback(notification, ct)).ToArray();
        var allTask = Task.WhenAll(tasks);

        try
        {
            await allTask.ConfigureAwait(false);
        }
        catch
        {
            throw allTask.Exception!;
        }
    }
}
