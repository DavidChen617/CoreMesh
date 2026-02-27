using System.Reflection;
using CoreMesh.Interception;

namespace CoreMesh.Interceptions.Tests;

public class IntegrationTests
{
    public interface ICalculator
    {
        int Add(int a, int b);
        Task<int> AddAsync(int a, int b);
        ValueTask<int> AddValueTaskAsync(int a, int b);
    }

    public class Calculator : ICalculator
    {
        public int Add(int a, int b) => a + b;

        public async Task<int> AddAsync(int a, int b)
        {
            await Task.Delay(1);
            return a + b;
        }

        public async ValueTask<int> AddValueTaskAsync(int a, int b)
        {
            await Task.Delay(1);
            return a + b;
        }
    }

    public class LoggingInterceptor(List<string> logs) : IInterceptor
    {
        public void BeforeInvoke(MethodInfo method, object?[]? args)
            => logs.Add($"[LOG] Before {method.Name}({string.Join(", ", args ?? [])})");

        public void AfterInvoke(MethodInfo method, object?[]? args, object? result)
            => logs.Add($"[LOG] After {method.Name} => {result}");

        public void OnError(MethodInfo method, object?[]? args, Exception error)
            => logs.Add($"[LOG] Error {method.Name}: {error.Message}");
    }

    public class TimingAsyncInterceptor(List<string> logs) : IAsyncInterceptor
    {
        public async ValueTask BeforeInvoke(MethodInfo method, object?[]? args)
        {
            await Task.Yield();
            logs.Add($"[TIMING] Start {method.Name}");
        }

        public async ValueTask AfterInvoke(MethodInfo method, object?[]? args, object? result)
        {
            await Task.Yield();
            logs.Add($"[TIMING] End {method.Name}");
        }

        public async ValueTask OnError(MethodInfo method, object?[]? args, Exception error)
        {
            await Task.Yield();
            logs.Add($"[TIMING] Error {method.Name}");
        }
    }

    [Fact]
    public void SyncProxy_WithSyncInterceptor_ShouldWorkEndToEnd()
    {
        var logs = new List<string>();
        var calculator = new Calculator();
        var interceptor = new LoggingInterceptor(logs);
        var proxy = InterceptorProxy<ICalculator>.Create(calculator, interceptor)!;

        var result = proxy.Add(5, 3);

        Assert.Equal(8, result);
        Assert.Equal(2, logs.Count);
        Assert.Equal("[LOG] Before Add(5, 3)", logs[0]);
        Assert.Equal("[LOG] After Add => 8", logs[1]);
    }

    [Fact]
    public async Task AsyncProxy_WithAsyncInterceptor_ShouldWorkEndToEnd()
    {
        var logs = new List<string>();
        var calculator = new Calculator();
        var interceptor = new TimingAsyncInterceptor(logs);
        var proxy = AsyncInterceptorProxy<ICalculator>.Create(calculator, interceptor)!;

        var result = await proxy.AddAsync(10, 20);

        Assert.Equal(30, result);
        Assert.Equal(2, logs.Count);
        Assert.Equal("[TIMING] Start AddAsync", logs[0]);
        Assert.Equal("[TIMING] End AddAsync", logs[1]);
    }

    [Fact]
    public async Task AsyncProxy_WithCompositeInterceptor_MixedTypes_ShouldWorkEndToEnd()
    {
        var logs = new List<string>();
        var calculator = new Calculator();
        var loggingInterceptor = new LoggingInterceptor(logs);
        var timingInterceptor = new TimingAsyncInterceptor(logs);

        var composite = new CompositeInterceptor(loggingInterceptor, timingInterceptor);
        var proxy = AsyncInterceptorProxy<ICalculator>.Create(calculator, composite)!;

        var result = await proxy.AddAsync(100, 200);

        Assert.Equal(300, result);
        // Should have both logging and timing entries
        Assert.Contains(logs, l => l.StartsWith("[LOG]"));
        Assert.Contains(logs, l => l.StartsWith("[TIMING]"));
    }

    [Fact]
    public void SyncProxy_WithCompositeInterceptor_MixedTypes_ShouldWorkEndToEnd()
    {
        var logs = new List<string>();
        var calculator = new Calculator();
        var loggingInterceptor = new LoggingInterceptor(logs);
        var timingInterceptor = new TimingAsyncInterceptor(logs);

        var composite = new CompositeInterceptor(loggingInterceptor, timingInterceptor);
        var proxy = InterceptorProxy<ICalculator>.Create(calculator, composite)!;

        var result = proxy.Add(1, 2);

        Assert.Equal(3, result);
        // Both interceptors should be called (async one will block)
        Assert.Contains(logs, l => l.StartsWith("[LOG]"));
        Assert.Contains(logs, l => l.StartsWith("[TIMING]"));
    }

    [Fact]
    public async Task ValueTaskMethod_ShouldWorkCorrectly()
    {
        var logs = new List<string>();
        var calculator = new Calculator();
        var interceptor = new LoggingInterceptor(logs);
        var proxy = InterceptorProxy<ICalculator>.Create(calculator, interceptor)!;

        var result = await proxy.AddValueTaskAsync(7, 8);

        Assert.Equal(15, result);
        Assert.Equal(2, logs.Count);
    }

    [Fact]
    public async Task MultipleInterceptors_ShouldExecuteInOrder()
    {
        var order = new List<int>();

        var interceptor1 = new OrderTrackingInterceptor(order, 1);
        var interceptor2 = new OrderTrackingInterceptor(order, 2);
        var interceptor3 = new OrderTrackingInterceptor(order, 3);

        var composite = new CompositeInterceptor(interceptor1, interceptor2, interceptor3);
        var calculator = new Calculator();
        var proxy = AsyncInterceptorProxy<ICalculator>.Create(calculator, composite)!;

        await proxy.AddAsync(1, 1);

        // Before should be in order: 1, 2, 3
        // After should be in order: 1, 2, 3 (not reversed)
        Assert.Equal([1, 2, 3, 1, 2, 3], order);
    }

    private class OrderTrackingInterceptor(List<int> order, int id) : IAsyncInterceptor
    {
        public ValueTask BeforeInvoke(MethodInfo method, object?[]? args)
        {
            order.Add(id);
            return default;
        }

        public ValueTask AfterInvoke(MethodInfo method, object?[]? args, object? result)
        {
            order.Add(id);
            return default;
        }

        public ValueTask OnError(MethodInfo method, object?[]? args, Exception error)
        {
            order.Add(id);
            return default;
        }
    }
}
