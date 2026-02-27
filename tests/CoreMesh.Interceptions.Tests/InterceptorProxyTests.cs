using System.Reflection;
using CoreMesh.Interception;

namespace CoreMesh.Interceptions.Tests;

public class InterceptorProxyTests
{
    public interface ITestService
    {
        int Add(int a, int b);
        void DoWork();
        Task DoWorkAsync();
        Task<int> AddAsync(int a, int b);
        ValueTask DoWorkValueTaskAsync();
        ValueTask<int> AddValueTaskAsync(int a, int b);
        void ThrowException();
        Task ThrowExceptionAsync();
    }

    public class TestService : ITestService
    {
        public int Add(int a, int b) => a + b;
        public void DoWork() { }
        public async Task DoWorkAsync() => await Task.Delay(1);

        public async Task<int> AddAsync(int a, int b)
        {
            await Task.Delay(1);
            return a + b;
        }

        public async ValueTask DoWorkValueTaskAsync() => await Task.Delay(1);

        public async ValueTask<int> AddValueTaskAsync(int a, int b)
        {
            await Task.Delay(1);
            return a + b;
        }

        public void ThrowException() => throw new InvalidOperationException("Test error");

        public async Task ThrowExceptionAsync()
        {
            await Task.Delay(1);
            throw new InvalidOperationException("Test async error");
        }
    }

    public class RecordingInterceptor : IInterceptor
    {
        public List<string> Calls { get; } = [];
        public MethodInfo? LastMethod { get; private set; }
        public object?[]? LastArgs { get; private set; }
        public object? LastResult { get; private set; }
        public Exception? LastError { get; private set; }

        public void BeforeInvoke(MethodInfo method, object?[]? args)
        {
            Calls.Add($"Before:{method.Name}");
            LastMethod = method;
            LastArgs = args;
        }

        public void AfterInvoke(MethodInfo method, object?[]? args, object? result)
        {
            Calls.Add($"After:{method.Name}");
            LastResult = result;
        }

        public void OnError(MethodInfo method, object?[]? args, Exception error)
        {
            Calls.Add($"Error:{method.Name}");
            LastError = error;
        }
    }

    [Fact]
    public void SyncMethod_ShouldCallBeforeAndAfter()
    {
        var service = new TestService();
        var interceptor = new RecordingInterceptor();
        var proxy = InterceptorProxy<ITestService>.Create(service, interceptor)!;

        var result = proxy.Add(1, 2);

        Assert.Equal(3, result);
        Assert.Equal(["Before:Add", "After:Add"], interceptor.Calls);
        Assert.Equal("Add", interceptor.LastMethod?.Name);
        Assert.Equal([1, 2], interceptor.LastArgs);
        Assert.Equal(3, interceptor.LastResult);
    }

    [Fact]
    public void SyncVoidMethod_ShouldCallBeforeAndAfter()
    {
        var service = new TestService();
        var interceptor = new RecordingInterceptor();
        var proxy = InterceptorProxy<ITestService>.Create(service, interceptor)!;

        proxy.DoWork();

        Assert.Equal(["Before:DoWork", "After:DoWork"], interceptor.Calls);
    }

    [Fact]
    public async Task AsyncTask_ShouldCallBeforeAndAfter()
    {
        var service = new TestService();
        var interceptor = new RecordingInterceptor();
        var proxy = InterceptorProxy<ITestService>.Create(service, interceptor)!;

        await proxy.DoWorkAsync();

        Assert.Equal(["Before:DoWorkAsync", "After:DoWorkAsync"], interceptor.Calls);
    }

    [Fact]
    public async Task AsyncTaskWithResult_ShouldCallBeforeAndAfterWithResult()
    {
        var service = new TestService();
        var interceptor = new RecordingInterceptor();
        var proxy = InterceptorProxy<ITestService>.Create(service, interceptor)!;

        var result = await proxy.AddAsync(5, 3);

        Assert.Equal(8, result);
        Assert.Equal(["Before:AddAsync", "After:AddAsync"], interceptor.Calls);
        Assert.Equal(8, interceptor.LastResult);
    }

    [Fact]
    public async Task AsyncValueTask_ShouldCallBeforeAndAfter()
    {
        var service = new TestService();
        var interceptor = new RecordingInterceptor();
        var proxy = InterceptorProxy<ITestService>.Create(service, interceptor)!;

        await proxy.DoWorkValueTaskAsync();

        Assert.Equal(["Before:DoWorkValueTaskAsync", "After:DoWorkValueTaskAsync"], interceptor.Calls);
    }

    [Fact]
    public async Task AsyncValueTaskWithResult_ShouldCallBeforeAndAfterWithResult()
    {
        var service = new TestService();
        var interceptor = new RecordingInterceptor();
        var proxy = InterceptorProxy<ITestService>.Create(service, interceptor)!;

        var result = await proxy.AddValueTaskAsync(10, 20);

        Assert.Equal(30, result);
        Assert.Equal(["Before:AddValueTaskAsync", "After:AddValueTaskAsync"], interceptor.Calls);
        Assert.Equal(30, interceptor.LastResult);
    }

    [Fact]
    public void SyncMethod_OnException_ShouldCallOnError()
    {
        var service = new TestService();
        var interceptor = new RecordingInterceptor();
        var proxy = InterceptorProxy<ITestService>.Create(service, interceptor)!;

        Assert.Throws<TargetInvocationException>(() => proxy.ThrowException());

        Assert.Contains("Before:ThrowException", interceptor.Calls);
        Assert.Contains("Error:ThrowException", interceptor.Calls);
        Assert.DoesNotContain("After:ThrowException", interceptor.Calls);
        Assert.IsType<InvalidOperationException>(interceptor.LastError);
    }

    [Fact]
    public async Task AsyncMethod_OnException_ShouldCallOnError()
    {
        var service = new TestService();
        var interceptor = new RecordingInterceptor();
        var proxy = InterceptorProxy<ITestService>.Create(service, interceptor)!;

        await Assert.ThrowsAsync<InvalidOperationException>(() => proxy.ThrowExceptionAsync());

        Assert.Contains("Before:ThrowExceptionAsync", interceptor.Calls);
        Assert.Contains("Error:ThrowExceptionAsync", interceptor.Calls);
        Assert.DoesNotContain("After:ThrowExceptionAsync", interceptor.Calls);
    }
}
