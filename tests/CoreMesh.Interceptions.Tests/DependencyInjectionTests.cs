using System.Reflection;
using CoreMesh.Interception;
using CoreMesh.Interception.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace CoreMesh.Interceptions.Tests;

public class DependencyInjectionTests
{
    public static List<string> Logs { get; } = [];

    public class TestLoggingInterceptor : IInterceptor
    {
        public void BeforeInvoke(MethodInfo method, object?[]? args)
            => Logs.Add($"Before:{method.Name}");

        public void AfterInvoke(MethodInfo method, object?[]? args, object? result)
            => Logs.Add($"After:{method.Name}");

        public void OnError(MethodInfo method, object?[]? args, Exception error)
            => Logs.Add($"Error:{method.Name}");
    }

    public class TestAsyncInterceptor : IAsyncInterceptor
    {
        public ValueTask BeforeInvoke(MethodInfo method, object?[]? args)
        {
            Logs.Add($"AsyncBefore:{method.Name}");
            return default;
        }

        public ValueTask AfterInvoke(MethodInfo method, object?[]? args, object? result)
        {
            Logs.Add($"AsyncAfter:{method.Name}");
            return default;
        }

        public ValueTask OnError(MethodInfo method, object?[]? args, Exception error)
        {
            Logs.Add($"AsyncError:{method.Name}");
            return default;
        }
    }

    [InterceptedBy<TestLoggingInterceptor>]
    public interface ISyncService
    {
        int Add(int a, int b);
    }

    public class SyncService : ISyncService
    {
        public int Add(int a, int b) => a + b;
    }

    [InterceptedBy<TestAsyncInterceptor>]
    public interface IAsyncService
    {
        Task<int> AddAsync(int a, int b);
    }

    public class AsyncService : IAsyncService
    {
        public async Task<int> AddAsync(int a, int b)
        {
            await Task.Delay(1);
            return a + b;
        }
    }

    [InterceptedBy<TestLoggingInterceptor>]
    [InterceptedBy<TestAsyncInterceptor>]
    public interface IMixedService
    {
        Task<string> GetMessageAsync(string name);
    }

    public class MixedService : IMixedService
    {
        public async Task<string> GetMessageAsync(string name)
        {
            await Task.Delay(1);
            return $"Hello, {name}!";
        }
    }

    public interface IInheritedService : IInterceptableBase
    {
        int Multiply(int a, int b);
    }

    [InterceptedBy<TestLoggingInterceptor>]
    public interface IInterceptableBase;

    public class InheritedService : IInheritedService
    {
        public int Multiply(int a, int b) => a * b;
    }

    [Fact]
    public void AddInterceptor_WithSyncInterceptor_ShouldWrapService()
    {
        Logs.Clear();
        var services = new ServiceCollection();
        services.AddSingleton<ISyncService, SyncService>();
        services.AddInterceptor(typeof(DependencyInjectionTests).Assembly!);

        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<ISyncService>();

        var result = service.Add(2, 3);

        Assert.Equal(5, result);
        Assert.Equal(["Before:Add", "After:Add"], Logs);
    }

    [Fact]
    public async Task AddInterceptor_WithAsyncInterceptor_ShouldWrapService()
    {
        Logs.Clear();
        var services = new ServiceCollection();
        services.AddSingleton<IAsyncService, AsyncService>();
        services.AddInterceptor(typeof(DependencyInjectionTests).Assembly!);

        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IAsyncService>();

        var result = await service.AddAsync(10, 20);

        Assert.Equal(30, result);
        Assert.Equal(["AsyncBefore:AddAsync", "AsyncAfter:AddAsync"], Logs);
    }

    [Fact]
    public async Task AddInterceptor_WithMixedInterceptors_ShouldWrapServiceWithBoth()
    {
        Logs.Clear();
        var services = new ServiceCollection();
        services.AddSingleton<IMixedService, MixedService>();
        services.AddInterceptor(typeof(DependencyInjectionTests).Assembly!);

        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IMixedService>();

        var result = await service.GetMessageAsync("World");

        Assert.Equal("Hello, World!", result);
        Assert.Contains("Before:GetMessageAsync", Logs);
        Assert.Contains("AsyncBefore:GetMessageAsync", Logs);
        Assert.Contains("After:GetMessageAsync", Logs);
        Assert.Contains("AsyncAfter:GetMessageAsync", Logs);
    }

    [Fact]
    public void AddInterceptor_WithInheritedAttribute_ShouldWrapService()
    {
        Logs.Clear();
        var services = new ServiceCollection();
        services.AddSingleton<IInheritedService, InheritedService>();
        services.AddInterceptor(typeof(DependencyInjectionTests).Assembly!);

        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IInheritedService>();

        var result = service.Multiply(3, 4);

        Assert.Equal(12, result);
        Assert.Equal(["Before:Multiply", "After:Multiply"], Logs);
    }

    [Fact]
    public void AddInterceptor_WithoutRegisteredService_ShouldNotThrow()
    {
        var services = new ServiceCollection();
        // ISyncService not registered
        services.AddInterceptor(typeof(DependencyInjectionTests).Assembly!);

        var provider = services.BuildServiceProvider();

        // Should return null since service wasn't registered
        var service = provider.GetService<ISyncService>();
        Assert.Null(service);
    }

    [Fact]
    public void AddInterceptor_ServiceLifetime_ShouldBePreserved()
    {
        var services = new ServiceCollection();
        services.AddScoped<ISyncService, SyncService>();
        services.AddInterceptor(typeof(DependencyInjectionTests).Assembly!);

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ISyncService));

        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void AddInterceptor_ShouldReturnSameServiceCollection()
    {
        var services = new ServiceCollection();
        var result = services.AddInterceptor([typeof(DependencyInjectionTests).Assembly]);

        Assert.Same(services, result);
    }
}
