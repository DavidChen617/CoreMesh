using CoreMesh.Dispatching.Notification;
using CoreMesh.Dispatching.Notification.Publisher;

namespace CoreMesh.Dispatching.Extensions;

/// <summary>
/// Options for configuring the dispatcher.
/// </summary>
public sealed class DispatchingOptions
{
    private Type _notificationPublisherType = typeof(ForeachAwaitPublisher);

    /// <summary>
    /// Gets or sets the type of <see cref="INotificationPublisher"/> to use.
    /// Defaults to <see cref="ForeachAwaitPublisher"/>.
    /// </summary>
    public Type NotificationPublisherType
    {
        get => _notificationPublisherType;
        set
        {
            if (!typeof(INotificationPublisher).IsAssignableFrom(value))
                throw new ArgumentException(
                    $"Type {value.FullName} must implement {nameof(INotificationPublisher)}",
                    nameof(value));

            _notificationPublisherType = value;
        }
    }

    /// <summary>
    /// Sets the notification publisher to execute handlers sequentially (one at a time).
    /// This is the default behavior.
    /// </summary>
    /// <returns>The options instance for chaining.</returns>
    public DispatchingOptions UseSequentialPublisher()
    {
        NotificationPublisherType = typeof(ForeachAwaitPublisher);
        return this;
    }

    /// <summary>
    /// Sets the notification publisher to execute handlers in parallel using <see cref="Task.WhenAll"/>.
    /// </summary>
    /// <returns>The options instance for chaining.</returns>
    public DispatchingOptions UseParallelPublisher()
    {
        NotificationPublisherType = typeof(TaskWhenAllPublisher);
        return this;
    }

    /// <summary>
    /// Sets a custom notification publisher type.
    /// </summary>
    /// <typeparam name="TPublisher">The publisher type that implements <see cref="INotificationPublisher"/>.</typeparam>
    /// <returns>The options instance for chaining.</returns>
    public DispatchingOptions UseCustomPublisher<TPublisher>() where TPublisher : INotificationPublisher
    {
        NotificationPublisherType = typeof(TPublisher);
        return this;
    }
}