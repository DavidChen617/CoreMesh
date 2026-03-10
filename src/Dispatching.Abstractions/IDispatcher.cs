namespace CoreMesh.Dispatching.Abstractions;

/// <summary>
/// Defines the combined request sending and notification publishing entry point.
/// </summary>
public interface IDispatcher : ISender, IPublisher;

/// <summary>
/// Defines the request sending entry point.
/// </summary>
public interface ISender
{
    /// <summary>
    /// Sends a request that does not return a response payload.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <param name="request">The request instance.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the handler finishes.</returns>
    Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest;

    /// <summary>
    /// Sends a request that returns a response.
    /// </summary>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="request">The request instance.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response produced by the handler.</returns>
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines the notification publishing entry point.
/// </summary>
public interface IPublisher
{
    /// <summary>
    /// Publishes a notification to all registered handlers.
    /// </summary>
    /// <param name="notification">The notification instance (must implement <see cref="INotification"/>).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when all handlers finish.</returns>
    Task Publish(object notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a strongly-typed notification to all registered handlers.
    /// </summary>
    /// <typeparam name="TNotification">The notification type.</typeparam>
    /// <param name="notification">The notification instance.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when all handlers finish.</returns>
    Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification;
}
