namespace CoreMesh.Dispatching.Request;

/// <summary>
/// Base class for request handler wrappers used internally by the dispatcher.
/// </summary>
public abstract class RequestHandlerBase
{
    /// <summary>
    /// Handles the request and returns the response as a boxed object.
    /// </summary>
    /// <param name="request">The request instance.</param>
    /// <param name="sp">The service provider.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The boxed response, or null for void requests.</returns>
    public abstract Task<object?> Handle(object request, IServiceProvider sp, CancellationToken ct = default);
}
