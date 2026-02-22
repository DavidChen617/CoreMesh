namespace CoreMesh.Dispatching.Wrappers;

/// <summary>
/// Base wrapper abstraction for request handler dispatching.
/// </summary>
public abstract class RequestHandlerBase
{
    /// <summary>
    /// Handles a request represented as <see cref="object"/>.
    /// </summary>
    /// <param name="request">The request instance.</param>
    /// <param name="serviceProvider">The service provider used to resolve handlers.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The boxed response, or <see langword="null"/> for void requests.</returns>
    public abstract Task<object?> Handle(
        object request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default);
}
