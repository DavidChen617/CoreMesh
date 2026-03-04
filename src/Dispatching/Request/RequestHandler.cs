namespace CoreMesh.Dispatching.Request;

/// <summary>
/// Typed request handler wrapper for requests that return a response.
/// </summary>
/// <typeparam name="TResponse">The response type.</typeparam>
public abstract class RequestHandler<TResponse> : RequestHandlerBase
{
    /// <summary>
    /// Handles the request and returns the typed response.
    /// </summary>
    /// <param name="request">The request instance.</param>
    /// <param name="sp">The service provider.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The response.</returns>
    public abstract Task<TResponse> Handle(IRequest<TResponse> request, IServiceProvider sp,
        CancellationToken ct = default);
}

/// <summary>
/// Typed request handler wrapper for requests that do not return a response payload.
/// </summary>
public abstract class RequestHandler : RequestHandlerBase
{
    /// <summary>
    /// Handles the request and returns <see cref="Unit"/>.
    /// </summary>
    /// <param name="request">The request instance.</param>
    /// <param name="sp">The service provider.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A <see cref="Unit"/> value.</returns>
    public abstract Task<Unit> Handle(IRequest request, IServiceProvider sp,
        CancellationToken ct = default);
}
