using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace CoreMesh.Dispatching.Benchmarks;

[MemoryDiagnoser]
public class DispatcherBenchmarks
{
    private readonly SampleQuery _query = new("Foo", "Bar");
    private readonly UserCreated _notification = new(42);

    private SampleHandler _directHandler = default!;

    private ServiceProvider _spSend = default!;
    private ServiceProvider _spPublish1 = default!;
    private ServiceProvider _spPublish2 = default!;
    private ServiceProvider _spPublish5 = default!;

    private IDispatcher _dispatcherSend = default!;
    private IDispatcher _dispatcherPublish1 = default!;
    private IDispatcher _dispatcherPublish2 = default!;
    private IDispatcher _dispatcherPublish5 = default!;

    [GlobalSetup]
    public void Setup()
    {
        _directHandler = new SampleHandler();

        (_spSend, _dispatcherSend) = BuildSendProvider();
        (_spPublish1, _dispatcherPublish1) = BuildPublishProvider(1);
        (_spPublish2, _dispatcherPublish2) = BuildPublishProvider(2);
        (_spPublish5, _dispatcherPublish5) = BuildPublishProvider(5);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _spSend.Dispose();
        _spPublish1.Dispose();
        _spPublish2.Dispose();
        _spPublish5.Dispose();
    }

    [Benchmark]
    public Task<SampleResponse> Baseline_DirectHandler()
        => _directHandler.Handle(_query, CancellationToken.None);

    [Benchmark]
    public Task<SampleResponse> Send()
        => _dispatcherSend.Send(_query, CancellationToken.None);

    [Benchmark]
    public Task Publish_1Handler()
        => _dispatcherPublish1.Publish(_notification, CancellationToken.None);

    [Benchmark]
    public Task Publish_2Handlers()
        => _dispatcherPublish2.Publish(_notification, CancellationToken.None);

    [Benchmark]
    public Task Publish_5Handlers()
        => _dispatcherPublish5.Publish(_notification, CancellationToken.None);

    private static (ServiceProvider Provider, IDispatcher Dispatcher) BuildSendProvider()
    {
        var services = new ServiceCollection();
        services.AddScoped<IDispatcher, Dispatcher>();
        services.AddScoped<IRequestHandler<SampleQuery, SampleResponse>, SampleHandler>();

        var sp = services.BuildServiceProvider(validateScopes: false);
        var dispatcher = sp.GetRequiredService<IDispatcher>();
        return (sp, dispatcher);
    }

    private static (ServiceProvider Provider, IDispatcher Dispatcher) BuildPublishProvider(int handlerCount)
    {
        var services = new ServiceCollection();
        services.AddScoped<IDispatcher, Dispatcher>();

        if (handlerCount >= 1) services.AddScoped<INotificationHandler<UserCreated>, NotificationHandler1>();
        if (handlerCount >= 2) services.AddScoped<INotificationHandler<UserCreated>, NotificationHandler2>();
        if (handlerCount >= 3) services.AddScoped<INotificationHandler<UserCreated>, NotificationHandler3>();
        if (handlerCount >= 4) services.AddScoped<INotificationHandler<UserCreated>, NotificationHandler4>();
        if (handlerCount >= 5) services.AddScoped<INotificationHandler<UserCreated>, NotificationHandler5>();

        var sp = services.BuildServiceProvider(validateScopes: false);
        var dispatcher = sp.GetRequiredService<IDispatcher>();
        return (sp, dispatcher);
    }

    public sealed record SampleQuery(string Foo, string Bar) : IRequest<SampleResponse>;

    public sealed record SampleResponse(string Foo, string Bar);

    public sealed record UserCreated(int UserId) : INotification;

    public sealed class SampleHandler : IRequestHandler<SampleQuery, SampleResponse>
    {
        public Task<SampleResponse> Handle(SampleQuery request, CancellationToken cancellationToken = default)
            => Task.FromResult(new SampleResponse(request.Foo, request.Bar));
    }

    public sealed class NotificationHandler1 : INotificationHandler<UserCreated>
    {
        public Task Handle(UserCreated notification, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    public sealed class NotificationHandler2 : INotificationHandler<UserCreated>
    {
        public Task Handle(UserCreated notification, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    public sealed class NotificationHandler3 : INotificationHandler<UserCreated>
    {
        public Task Handle(UserCreated notification, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    public sealed class NotificationHandler4 : INotificationHandler<UserCreated>
    {
        public Task Handle(UserCreated notification, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    public sealed class NotificationHandler5 : INotificationHandler<UserCreated>
    {
        public Task Handle(UserCreated notification, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
