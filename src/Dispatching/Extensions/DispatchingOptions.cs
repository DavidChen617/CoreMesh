using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Dispatching.Notification;
using CoreMesh.Dispatching.Notification.Publisher;

namespace CoreMesh.Dispatching.Extensions;

/// <summary>
/// Options for configuring the dispatcher.
/// </summary>
public sealed class DispatchingOptions
{
    /// <summary>
    /// Gets or sets the type of <see cref="INotificationPublisher"/> to use.
    /// Defaults to <see cref="ForeachAwaitPublisher"/>.
    /// </summary>
    public Type NotificationPublisherType
    {
        get;
        set
        {
            if (!typeof(INotificationPublisher).IsAssignableFrom(value))
                throw new ArgumentException(
                    $"Type {value.FullName} must implement {nameof(INotificationPublisher)}",
                    nameof(value));

            field = value;
        }
    } = typeof(ForeachAwaitPublisher);

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
    /// Sets the notification publisher to execute handlers in parallel using <see cref="Task.WhenAll(IEnumerable{Task})"/>.
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
