using CoreMesh.Dispatching.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace CoreMesh.Dispatching.Request;

/// <summary>
/// Typed request handler wrapper that resolves and invokes the handler for a request with response.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public class RequestHandlerImpl<TRequest, TResponse> : RequestHandler<TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <inheritdoc />
    public override async Task<object?> Handle(object request, IServiceProvider sp, CancellationToken ct = default) =>
        await Handle((IRequest<TResponse>)request, sp, ct);

    /// <inheritdoc />
    public override Task<TResponse> Handle(IRequest<TResponse> request, IServiceProvider sp,
        CancellationToken ct = default)
        => sp.GetRequiredService<IRequestHandler<TRequest, TResponse>>().Handle((TRequest)request, ct);
}

/// <summary>
/// Typed request handler wrapper that resolves and invokes the handler for a void request.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
public class RequestHandlerImpl<TRequest> : RequestHandler
    where TRequest : IRequest
{
    /// <inheritdoc />
    public override async Task<object?> Handle(object request, IServiceProvider sp, CancellationToken ct = default)
        => await Handle((IRequest)request, sp, ct).ConfigureAwait(false);

    /// <inheritdoc />
    public override async Task<Unit> Handle(IRequest request, IServiceProvider sp, CancellationToken ct = default)
    {
        await sp.GetRequiredService<IRequestHandler<TRequest>>()
            .Handle((TRequest)request, ct);

        return Unit.Value;
    }
}
