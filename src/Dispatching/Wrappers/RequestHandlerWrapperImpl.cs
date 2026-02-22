using Microsoft.Extensions.DependencyInjection;

namespace CoreMesh.Dispatching.Wrappers;

public sealed class RequestHandlerWrapperImpl<TRequest, TResponse> : RequestHandlerWrapper<TResponse>
    where TRequest : IRequest<TResponse>
{
    public override Task<TResponse> Handle(IRequest<TResponse> request, IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        return serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>()
            .Handle((TRequest)request, cancellationToken);
    }

    public override async Task<object?> Handle(object request, IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        return await Handle((TRequest)request, serviceProvider, cancellationToken).ConfigureAwait(false);
    }
}

public sealed class RequestHandlerWrapperImpl<TRequest> : RequestHandlerWrapper
    where TRequest : IRequest
{
    public override async Task<object?> Handle(
        object request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        await Handle((IRequest)request, serviceProvider, cancellationToken).ConfigureAwait(false);
        return null;
    }

    public override Task Handle(
        IRequest request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest>>();
        return handler.Handle((TRequest)request, cancellationToken);
    }
}
