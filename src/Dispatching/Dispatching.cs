using System.Collections.Concurrent;
using System.Linq.Expressions;
using CoreMesh.Dispatching.Wrappers;

namespace CoreMesh.Dispatching;

public sealed class Dispatcher(IServiceProvider serviceProvider) : IDispatcher
{
    private static readonly ConcurrentDictionary<Type, RequestHandlerBase> RequestHandlers = new();
    private static readonly ConcurrentDictionary<Type, NotificationHandlerWrapper> NotificationHandlers = new();
    private static readonly ConcurrentDictionary<Type, Func<RequestHandlerBase>> RequestFactoryCache = new();
    private static readonly ConcurrentDictionary<Type, Func<RequestHandlerBase>> VoidRequestFactoryCache = new();
    private static readonly ConcurrentDictionary<Type, Func<NotificationHandlerWrapper>> NotificationFactoryCache = new();

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var wrapper = (RequestHandlerWrapper<TResponse>)RequestHandlers.GetOrAdd(request.GetType(), static requestType =>
        {
            var factory = RequestFactoryCache.GetOrAdd(requestType, static requestType =>
            {
                var wrapperType = typeof(RequestHandlerWrapperImpl<,>).MakeGenericType(requestType, typeof(TResponse));
                return BuildRequestFactory(wrapperType);
            });

            return factory();
        });

        return wrapper.Handle(request, serviceProvider, cancellationToken);
    }

    public Task Send(IRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var wrapper = (RequestHandlerWrapper)RequestHandlers.GetOrAdd(request.GetType(), static requestType =>
        {
            var factory = VoidRequestFactoryCache.GetOrAdd(requestType, static requestType =>
            {
                var wrapperType = typeof(RequestHandlerWrapperImpl<>).MakeGenericType(requestType);
                return BuildRequestFactory(wrapperType);
            });
            return factory();
        });

        return wrapper.Handle(request, serviceProvider, cancellationToken);
    }

    public Task Publish(INotification notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);

        var wrapper = NotificationHandlers.GetOrAdd(notification.GetType(), static notificationType =>
        {
            var factory = NotificationFactoryCache.GetOrAdd(notificationType, static notificationType =>
            {
                var wrapperType = typeof(NotificationHandlerWrapperImpl<>).MakeGenericType(notificationType);
                return BuildNotificationFactory(wrapperType);
            });

            return factory();
        });

        return wrapper.Handle(notification, serviceProvider, cancellationToken);
    }

    private static Func<RequestHandlerBase> BuildRequestFactory(Type wrapperType)
    {
        var ctor = wrapperType.GetConstructor(Type.EmptyTypes) ??
                    throw new ArgumentException($"No parameterless ctor for {wrapperType.FullName}");
        var newExpr = Expression.New(ctor);
        var castExpr = Expression.Convert(newExpr, typeof(RequestHandlerBase));

        return Expression.Lambda<Func<RequestHandlerBase>>(castExpr).Compile();
    }

    private static Func<NotificationHandlerWrapper> BuildNotificationFactory(Type wrapperType)
    {
        var ctor = wrapperType.GetConstructor(Type.EmptyTypes) ??
                    throw new ArgumentException($"No parameterless ctor for {wrapperType.FullName}");
        var newExpr = Expression.New(ctor);
        var castExpr = Expression.Convert(newExpr, typeof(NotificationHandlerWrapper));

        return Expression.Lambda<Func<NotificationHandlerWrapper>>(castExpr).Compile();
    }
}
