namespace CoreMesh.Dispatching.Wrappers;

public abstract class RequestHandlerBase
{
    public abstract Task<object?> Handle(
        object request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default);
}
