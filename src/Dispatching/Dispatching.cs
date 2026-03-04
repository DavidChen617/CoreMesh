using System.Collections.Concurrent;
using System.Linq.Expressions;
using CoreMesh.Dispatching.Notification;
using CoreMesh.Dispatching.Request;

namespace CoreMesh.Dispatching;

/// <summary>
/// Default dispatcher implementation for request and notification dispatching.
/// </summary>
/// <param name="sp">The service provider used to resolve handlers.</param>
/// <param name="publisher">The notification publisher strategy.</param>
public sealed class Dispatcher(IServiceProvider sp, INotificationPublisher publisher) : IDispatcher
{
    private static readonly ConcurrentDictionary<Type, RequestHandlerBase> RequestHandlers = new();
    private static readonly ConcurrentDictionary<Type, NotificationHandler> NotificationHandlers = new();

    /// <inheritdoc />
    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest
    {
        ArgumentNullException.ThrowIfNull(request);

        var handler = (RequestHandler)RequestHandlers.GetOrAdd(request.GetType(),
            static requestType => CompileAndCreateInstance<RequestHandlerBase>(
                typeof(RequestHandlerImpl<>).MakeGenericType(requestType))
        );

        return handler.Handle(request, sp, cancellationToken);
    }

    /// <inheritdoc />
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var handler = (RequestHandler<TResponse>)RequestHandlers.GetOrAdd(request.GetType(),
            static requestType => CompileAndCreateInstance<RequestHandlerBase>(
                typeof(RequestHandlerImpl<,>).MakeGenericType(requestType, typeof(TResponse)))
        );

        return handler.Handle(request, sp, cancellationToken);
    }

    /// <inheritdoc />
    public Task Publish(object notification, CancellationToken cancellationToken = default)
    {
        if (notification is not INotification)
            throw new ArgumentException("Notification is not of type INotification", nameof(notification));

        return Publish((INotification)notification, cancellationToken);
    }

    /// <inheritdoc />
    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        if (notification is null)
            throw new ArgumentNullException(nameof(notification));

        var handler = NotificationHandlers.GetOrAdd(
            notification.GetType(),
            static notificationType => CompileAndCreateInstance<NotificationHandler>(
                typeof(NotificationHandlerImpl<>).MakeGenericType(notificationType))
        );

        return handler.Handle(notification, sp, publisher.Publish, cancellationToken);
    }

    private static THandlerType CompileAndCreateInstance<THandlerType>(Type wrapperType)
    {
        var ctor = wrapperType.GetConstructor(Type.EmptyTypes)
                   ?? throw new InvalidOperationException($"No parameterless constructor for {wrapperType}");
        var cast = Expression.Convert(Expression.New(ctor), typeof(THandlerType));

        var lambda = Expression.Lambda<Func<THandlerType>>(cast);
        var expressionTree = lambda.Compile();
        var instance = expressionTree();
        return instance;
    }
}
