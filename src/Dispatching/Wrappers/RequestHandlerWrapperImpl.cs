using Microsoft.Extensions.DependencyInjection;

namespace CoreMesh.Dispatching.Wrappers;

/// <summary>
/// Wrapper implementation for requests that return a response.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class RequestHandlerWrapperImpl<TRequest, TResponse> : RequestHandlerWrapper<TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <inheritdoc />
    public override Task<TResponse> Handle(IRequest<TResponse> request, IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        return serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>()
            .Handle((TRequest)request, cancellationToken);
    }

    /// <inheritdoc />
    public override async Task<object?> Handle(object request, IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        return await Handle((TRequest)request, serviceProvider, cancellationToken).ConfigureAwait(false);
    }
}

/// <summary>
/// Wrapper implementation for requests that do not return a response payload.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
public sealed class RequestHandlerWrapperImpl<TRequest> : RequestHandlerWrapper
    where TRequest : IRequest
{
    /// <inheritdoc />
    public override async Task<object?> Handle(
        object request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        await Handle((IRequest)request, serviceProvider, cancellationToken).ConfigureAwait(false);
        return null;
    }

    /// <inheritdoc />
    public override Task Handle(
        IRequest request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest>>();
        return handler.Handle((TRequest)request, cancellationToken);
    }
}
