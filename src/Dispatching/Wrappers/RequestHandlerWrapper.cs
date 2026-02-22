namespace CoreMesh.Dispatching.Wrappers;

public abstract class RequestHandlerWrapper<TResponse> : RequestHandlerBase
{
    public abstract Task<TResponse> Handle(
        IRequest<TResponse> request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default);
}

public abstract class RequestHandlerWrapper : RequestHandlerBase
{
    public abstract Task Handle(
        IRequest request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}
