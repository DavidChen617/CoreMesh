using System.Reflection;
using BenchmarkDotNet.Attributes;
using Castle.DynamicProxy;
using CoreMesh.Interception;

namespace CoreMesh.Interceptions.Benchmark;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, invocationCount: 10)]
public class InterceptionBenchmark
{
    private IMyService _direct = null!;
    private IMyService _syncProxy = null!;
    private IMyService _asyncProxy = null!;
    private IMyService _castleProxy = null!;

    [GlobalSetup]
    public void Setup()
    {
        _direct = new MyService();

        // 同步 Interceptor（IInterceptor - void）
        _syncProxy = InterceptorProxy<IMyService>.Create(
            new MyService(),
            new EmptyInterceptor())!;

        // 異步 Interceptor（IAsyncInterceptor - ValueTask）
        _asyncProxy = AsyncInterceptorProxy<IMyService>.Create(
            new MyService(),
            new EmptyAsyncInterceptor())!;

        // Castle DynamicProxy
        var generator = new ProxyGenerator();
        _castleProxy = generator.CreateInterfaceProxyWithTarget<IMyService>(
            new MyService(),
            new EmptyCastleInterceptor());
    }

    [Benchmark(Baseline = true)]
    public int Direct() => _direct.Add(1, 2);

    [Benchmark]
    public int SyncInterceptor() => _syncProxy.Add(1, 2);

    [Benchmark]
    public int AsyncInterceptor() => _asyncProxy.Add(1, 2);

    [Benchmark]
    public int CastleDynamicProxy() => _castleProxy.Add(1, 2);
}

/// <summary>
/// 同步 Interceptor（IInterceptor - void）
/// </summary>
public class EmptyInterceptor : Interception.IInterceptor
{
    public void BeforeInvoke(MethodInfo method, object?[]? args) { }
    public void AfterInvoke(MethodInfo method, object?[]? args, object? result) { }
    public void OnError(MethodInfo method, object?[]? args, Exception error) { }
}

/// <summary>
/// 異步 Interceptor（IAsyncInterceptor - ValueTask）
/// </summary>
public class EmptyAsyncInterceptor : IAsyncInterceptor
{
    public ValueTask BeforeInvoke(MethodInfo method, object?[]? args) => default;
    public ValueTask AfterInvoke(MethodInfo method, object?[]? args, object? result) => default;
    public ValueTask OnError(MethodInfo method, object?[]? args, Exception error) => default;
}

public class EmptyCastleInterceptor : Castle.DynamicProxy.IInterceptor
{
    public void Intercept(IInvocation invocation) => invocation.Proceed();
}


public interface IMyService
{
    int Add(int a, int b);
}

public class MyService : IMyService
{
    public int Add(int a, int b) => a + b;
}
