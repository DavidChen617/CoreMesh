namespace CoreMesh.Dispatching.Wrappers;

/// <summary>
/// Base wrapper for requests that return a response.
/// </summary>
/// <typeparam name="TResponse">The response type.</typeparam>
public abstract class RequestHandlerWrapper<TResponse> : RequestHandlerBase
{
    /// <summary>
    /// Handles the specified request.
    /// </summary>
    /// <param name="request">The request instance.</param>
    /// <param name="serviceProvider">The service provider used to resolve handlers.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The handler response.</returns>
    public abstract Task<TResponse> Handle(
        IRequest<TResponse> request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Base wrapper for requests that do not return a response payload.
/// </summary>
public abstract class RequestHandlerWrapper : RequestHandlerBase
{
    /// <summary>
    /// Handles the specified request.
    /// </summary>
    /// <param name="request">The request instance.</param>
    /// <param name="serviceProvider">The service provider used to resolve handlers.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the handler finishes.</returns>
    public abstract Task Handle(
        IRequest request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}
