using BenchmarkDotNet.Attributes;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Dispatching.Notification.Publisher;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using INotification = CoreMesh.Dispatching.Abstractions.INotification;
using INotificationPublisher = CoreMesh.Dispatching.Abstractions.INotificationPublisher;

namespace CoreMesh.Dispatching.Benchmarks;

[MemoryDiagnoser]
public class DispatcherBenchmarks
{
    // CoreMesh
    private readonly SampleQuery _query = new("Foo", "Bar");
    private readonly UserCreated _notification = new(42);

    // MediatR
    private readonly MediatRSampleQuery _mediatRQuery = new("Foo", "Bar");
    private readonly MediatRUserCreated _mediatRNotification = new(42);

    private SampleHandler _directHandler = default!;

    // CoreMesh providers
    private ServiceProvider _spSend = default!;
    private ServiceProvider _spPublish1 = default!;
    private ServiceProvider _spPublish2 = default!;
    private ServiceProvider _spPublish5 = default!;

    private IDispatcher _dispatcherSend = default!;
    private IDispatcher _dispatcherPublish1 = default!;
    private IDispatcher _dispatcherPublish2 = default!;
    private IDispatcher _dispatcherPublish5 = default!;

    // MediatR providers
    private ServiceProvider _mediatRSpSend = default!;
    private ServiceProvider _mediatRSpPublish1 = default!;
    private ServiceProvider _mediatRSpPublish2 = default!;
    private ServiceProvider _mediatRSpPublish5 = default!;

    private IMediator _mediatorSend = default!;
    private IMediator _mediatorPublish1 = default!;
    private IMediator _mediatorPublish2 = default!;
    private IMediator _mediatorPublish5 = default!;

    [GlobalSetup]
    public void Setup()
    {
        _directHandler = new SampleHandler();

        // CoreMesh setup
        (_spSend, _dispatcherSend) = BuildSendProvider();
        (_spPublish1, _dispatcherPublish1) = BuildPublishProvider(1);
        (_spPublish2, _dispatcherPublish2) = BuildPublishProvider(2);
        (_spPublish5, _dispatcherPublish5) = BuildPublishProvider(5);

        // MediatR setup
        (_mediatRSpSend, _mediatorSend) = BuildMediatRSendProvider();
        (_mediatRSpPublish1, _mediatorPublish1) = BuildMediatRPublishProvider(1);
        (_mediatRSpPublish2, _mediatorPublish2) = BuildMediatRPublishProvider(2);
        (_mediatRSpPublish5, _mediatorPublish5) = BuildMediatRPublishProvider(5);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _spSend.Dispose();
        _spPublish1.Dispose();
        _spPublish2.Dispose();
        _spPublish5.Dispose();

        _mediatRSpSend.Dispose();
        _mediatRSpPublish1.Dispose();
        _mediatRSpPublish2.Dispose();
        _mediatRSpPublish5.Dispose();
    }

    // ========== Baseline ==========

    [Benchmark(Baseline = true)]
    public Task<SampleResponse> Baseline_DirectHandler()
        => _directHandler.Handle(_query, CancellationToken.None);

    // ========== CoreMesh Send ==========

    [Benchmark]
    public Task<SampleResponse> CoreMesh_Send()
        => _dispatcherSend.Send(_query, CancellationToken.None);

    // ========== MediatR Send ==========

    [Benchmark]
    public Task<MediatRSampleResponse> MediatR_Send()
        => _mediatorSend.Send(_mediatRQuery, CancellationToken.None);

    // ========== CoreMesh Publish ==========

    [Benchmark]
    public Task CoreMesh_Publish_1Handler()
        => _dispatcherPublish1.Publish(_notification, CancellationToken.None);

    [Benchmark]
    public Task CoreMesh_Publish_2Handlers()
        => _dispatcherPublish2.Publish(_notification, CancellationToken.None);

    [Benchmark]
    public Task CoreMesh_Publish_5Handlers()
        => _dispatcherPublish5.Publish(_notification, CancellationToken.None);

    // ========== MediatR Publish ==========

    [Benchmark]
    public Task MediatR_Publish_1Handler()
        => _mediatorPublish1.Publish(_mediatRNotification, CancellationToken.None);

    [Benchmark]
    public Task MediatR_Publish_2Handlers()
        => _mediatorPublish2.Publish(_mediatRNotification, CancellationToken.None);

    [Benchmark]
    public Task MediatR_Publish_5Handlers()
        => _mediatorPublish5.Publish(_mediatRNotification, CancellationToken.None);

    private static (ServiceProvider Provider, IDispatcher Dispatcher) BuildSendProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<INotificationPublisher, ForeachAwaitPublisher>();
        services.AddScoped<IDispatcher, Dispatcher>();
        services.AddScoped<Abstractions.IRequestHandler<SampleQuery, SampleResponse>, SampleHandler>();

        var sp = services.BuildServiceProvider(validateScopes: false);
        var dispatcher = sp.GetRequiredService<IDispatcher>();
        return (sp, dispatcher);
    }

    private static (ServiceProvider Provider, IDispatcher Dispatcher) BuildPublishProvider(int handlerCount)
    {
        var services = new ServiceCollection();
        services.AddSingleton<INotificationPublisher, ForeachAwaitPublisher>();
        services.AddScoped<IDispatcher, Dispatcher>();

        if (handlerCount >= 1) services.AddScoped<Abstractions.INotificationHandler<UserCreated>, NotificationHandler1>();
        if (handlerCount >= 2) services.AddScoped<Abstractions.INotificationHandler<UserCreated>, NotificationHandler2>();
        if (handlerCount >= 3) services.AddScoped<Abstractions.INotificationHandler<UserCreated>, NotificationHandler3>();
        if (handlerCount >= 4) services.AddScoped<Abstractions.INotificationHandler<UserCreated>, NotificationHandler4>();
        if (handlerCount >= 5) services.AddScoped<Abstractions.INotificationHandler<UserCreated>, NotificationHandler5>();

        var sp = services.BuildServiceProvider(validateScopes: false);
        var dispatcher = sp.GetRequiredService<IDispatcher>();
        return (sp, dispatcher);
    }

    // ========== MediatR Providers ==========

    private static (ServiceProvider Provider, IMediator Mediator) BuildMediatRSendProvider()
    {
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<MediatRSampleHandler>());

        var sp = services.BuildServiceProvider(validateScopes: false);
        var mediator = sp.GetRequiredService<IMediator>();
        return (sp, mediator);
    }

    private static (ServiceProvider Provider, IMediator Mediator) BuildMediatRPublishProvider(int handlerCount)
    {
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<MediatRSampleHandler>());

        // Remove auto-registered handlers and add only the count we need
        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(MediatR.INotificationHandler<MediatRUserCreated>));
        while (descriptor != null)
        {
            services.Remove(descriptor);
            descriptor = services.FirstOrDefault(d =>
                d.ServiceType == typeof(MediatR.INotificationHandler<MediatRUserCreated>));
        }

        if (handlerCount >= 1)
            services.AddTransient<MediatR.INotificationHandler<MediatRUserCreated>, MediatRNotificationHandler1>();
        if (handlerCount >= 2)
            services.AddTransient<MediatR.INotificationHandler<MediatRUserCreated>, MediatRNotificationHandler2>();
        if (handlerCount >= 3)
            services.AddTransient<MediatR.INotificationHandler<MediatRUserCreated>, MediatRNotificationHandler3>();
        if (handlerCount >= 4)
            services.AddTransient<MediatR.INotificationHandler<MediatRUserCreated>, MediatRNotificationHandler4>();
        if (handlerCount >= 5)
            services.AddTransient<MediatR.INotificationHandler<MediatRUserCreated>, MediatRNotificationHandler5>();

        var sp = services.BuildServiceProvider(validateScopes: false);
        var mediator = sp.GetRequiredService<IMediator>();
        return (sp, mediator);
    }

    // ========== CoreMesh Types ==========

    public sealed record SampleQuery(string Foo, string Bar) : Abstractions.IRequest<SampleResponse>;

    public sealed record SampleResponse(string Foo, string Bar);

    public sealed record UserCreated(int UserId) : INotification;

    public sealed class SampleHandler : Abstractions.IRequestHandler<SampleQuery, SampleResponse>
    {
        public Task<SampleResponse> Handle(SampleQuery request, CancellationToken cancellationToken = default)
            => Task.FromResult(new SampleResponse(request.Foo, request.Bar));
    }

    public sealed class NotificationHandler1 : Abstractions.INotificationHandler<UserCreated>
    {
        public Task Handle(UserCreated notification, CancellationToken ct) => Task.CompletedTask;
    }

    public sealed class NotificationHandler2 : Abstractions.INotificationHandler<UserCreated>
    {
        public Task Handle(UserCreated notification, CancellationToken ct) => Task.CompletedTask;
    }

    public sealed class NotificationHandler3 : Abstractions.INotificationHandler<UserCreated>
    {
        public Task Handle(UserCreated notification, CancellationToken ct) => Task.CompletedTask;
    }

    public sealed class NotificationHandler4 : Abstractions.INotificationHandler<UserCreated>
    {
        public Task Handle(UserCreated notification, CancellationToken ct) => Task.CompletedTask;
    }

    public sealed class NotificationHandler5 : Abstractions.INotificationHandler<UserCreated>
    {
        public Task Handle(UserCreated notification, CancellationToken ct) => Task.CompletedTask;
    }

    // ========== MediatR Types ==========

    public sealed record MediatRSampleQuery(string Foo, string Bar) : MediatR.IRequest<MediatRSampleResponse>;

    public sealed record MediatRSampleResponse(string Foo, string Bar);

    public sealed record MediatRUserCreated(int UserId) : MediatR.INotification;

    public sealed class MediatRSampleHandler : MediatR.IRequestHandler<MediatRSampleQuery, MediatRSampleResponse>
    {
        public Task<MediatRSampleResponse> Handle(MediatRSampleQuery request, CancellationToken cancellationToken)
            => Task.FromResult(new MediatRSampleResponse(request.Foo, request.Bar));
    }

    public sealed class MediatRNotificationHandler1 : MediatR.INotificationHandler<MediatRUserCreated>
    {
        public Task Handle(MediatRUserCreated notification, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public sealed class MediatRNotificationHandler2 : MediatR.INotificationHandler<MediatRUserCreated>
    {
        public Task Handle(MediatRUserCreated notification, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public sealed class MediatRNotificationHandler3 : MediatR.INotificationHandler<MediatRUserCreated>
    {
        public Task Handle(MediatRUserCreated notification, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public sealed class MediatRNotificationHandler4 : MediatR.INotificationHandler<MediatRUserCreated>
    {
        public Task Handle(MediatRUserCreated notification, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public sealed class MediatRNotificationHandler5 : MediatR.INotificationHandler<MediatRUserCreated>
    {
        public Task Handle(MediatRUserCreated notification, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
