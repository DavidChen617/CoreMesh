using CoreMesh.Dispatching.Extensions;
using CoreMesh.Dispatching.Notification;
using CoreMesh.Dispatching.Notification.Publisher;
using Microsoft.Extensions.DependencyInjection;

namespace CoreMesh.Dispatching.Tests;

public class DispatchingTests
{
    [Fact]
    public async Task Send_Should_Return_Response()
    {
        var services = new ServiceCollection();

        services.AddDispatching([typeof(DispatchingTests).Assembly]);
        using var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IDispatcher>();

        var result = await dispatcher.Send(new PingQuery("foo"), CancellationToken.None);

        Assert.Equal("foo", result.Value);
    }

    [Fact]
    public async Task Send_Should_Throw_When_Handler_Not_Registered()
    {
        var services = new ServiceCollection();
        services.AddSingleton<INotificationPublisher, ForeachAwaitPublisher>();
        services.AddScoped<IDispatcher, Dispatcher>();
        // 不註冊 UnregisteredQuery 的 handler

        using var provider = services.BuildServiceProvider();

        var dispatcher = provider.GetRequiredService<IDispatcher>();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            dispatcher.Send(new UnregisteredQuery("foo"), CancellationToken.None));
    }

    [Fact]
    public async Task Publish_Should_Invoke_Handlers_In_Registration_Order_Sequentially()
    {
        var calls = new List<string>();

        var services = new ServiceCollection();
        services.AddSingleton<INotificationPublisher, ForeachAwaitPublisher>();
        services.AddScoped<IDispatcher, Dispatcher>();
        services.AddSingleton(calls);

        services.AddScoped<INotificationHandler<UserCreated>, FirstUserCreatedHandler>();
        services.AddScoped<INotificationHandler<UserCreated>, SecondUserCreatedHandler>();

        using var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IDispatcher>();

        await dispatcher.Publish(new UserCreated(123), CancellationToken.None);

        Assert.Equal(["first", "second"], calls);
    }

    [Fact]
    public async Task AddDispatching_With_Assembly_Should_Register_Handlers()
    {
        var services = new ServiceCollection();
        services.AddDispatching([typeof(PingHandler).Assembly]);

        using var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IDispatcher>();

        var result = await dispatcher.Send(new PingQuery("bar"), CancellationToken.None);

        Assert.Equal("bar", result.Value);
    }

    public sealed record PingQuery(string Value) : IRequest<PingResponse>;

    public sealed record PingResponse(string Value);

    private sealed record UnregisteredQuery(string Value) : IRequest<UnregisteredResponse>;

    private sealed record UnregisteredResponse(string Value);

    private sealed class PingHandler : IRequestHandler<PingQuery, PingResponse>
    {
        public Task<PingResponse> Handle(PingQuery request, CancellationToken cancellationToken = default)
            => Task.FromResult(new PingResponse(request.Value));
    }

    public sealed record UserCreated(int UserId) : INotification;

    public sealed class FirstUserCreatedHandler(List<string> calls) : INotificationHandler<UserCreated>
    {
        public Task Handle(UserCreated notification, CancellationToken cancellationToken = default)
        {
            calls.Add("first");
            return Task.CompletedTask;
        }
    }

    public sealed class SecondUserCreatedHandler(List<string> calls) : INotificationHandler<UserCreated>
    {
        public Task Handle(UserCreated notification, CancellationToken cancellationToken = default)
        {
            calls.Add("second");
            return Task.CompletedTask;
        }
    }
}
