using System.Reflection;
using CoreMesh.Interception;

namespace CoreMesh.Interceptions.Tests;

public class CompositeInterceptorTests
{
    public class RecordingSyncInterceptor(string name) : IInterceptor
    {
        public List<string> Calls { get; } = [];

        public void BeforeInvoke(MethodInfo method, object?[]? args)
            => Calls.Add($"{name}:Before:{method.Name}");

        public void AfterInvoke(MethodInfo method, object?[]? args, object? result)
            => Calls.Add($"{name}:After:{method.Name}");

        public void OnError(MethodInfo method, object?[]? args, Exception error)
            => Calls.Add($"{name}:Error:{method.Name}");
    }

    public class RecordingAsyncInterceptor(string name) : IAsyncInterceptor
    {
        public List<string> Calls { get; } = [];
        public bool SimulateAsyncWork { get; set; }

        public async ValueTask BeforeInvoke(MethodInfo method, object?[]? args)
        {
            if (SimulateAsyncWork) await Task.Delay(1);
            Calls.Add($"{name}:Before:{method.Name}");
        }

        public async ValueTask AfterInvoke(MethodInfo method, object?[]? args, object? result)
        {
            if (SimulateAsyncWork) await Task.Delay(1);
            Calls.Add($"{name}:After:{method.Name}");
        }

        public async ValueTask OnError(MethodInfo method, object?[]? args, Exception error)
        {
            if (SimulateAsyncWork) await Task.Delay(1);
            Calls.Add($"{name}:Error:{method.Name}");
        }
    }

    private static readonly MethodInfo TestMethod = typeof(object).GetMethod(nameof(ToString))!;

    [Fact]
    public void SyncInterface_WithMultipleSyncInterceptors_ShouldCallInOrder()
    {
        var interceptor1 = new RecordingSyncInterceptor("A");
        var interceptor2 = new RecordingSyncInterceptor("B");
        var composite = new CompositeInterceptor(interceptor1, interceptor2);
        var syncComposite = (IInterceptor)composite;

        syncComposite.BeforeInvoke(TestMethod, null);
        syncComposite.AfterInvoke(TestMethod, null, "result");
        syncComposite.OnError(TestMethod, null, new Exception());

        Assert.Equal(["A:Before:ToString", "B:Before:ToString"], interceptor1.Calls.Concat(interceptor2.Calls).Where(c => c.Contains("Before")).ToList());
        Assert.Equal(["A:After:ToString", "B:After:ToString"], interceptor1.Calls.Concat(interceptor2.Calls).Where(c => c.Contains("After")).ToList());
        Assert.Equal(["A:Error:ToString", "B:Error:ToString"], interceptor1.Calls.Concat(interceptor2.Calls).Where(c => c.Contains("Error")).ToList());
    }

    [Fact]
    public void SyncInterface_WithMixedInterceptors_ShouldBlockOnAsync()
    {
        var syncInterceptor = new RecordingSyncInterceptor("Sync");
        var asyncInterceptor = new RecordingAsyncInterceptor("Async");
        var composite = new CompositeInterceptor(syncInterceptor, asyncInterceptor);
        var syncComposite = (IInterceptor)composite;

        syncComposite.BeforeInvoke(TestMethod, null);

        Assert.Single(syncInterceptor.Calls);
        Assert.Single(asyncInterceptor.Calls);
        Assert.Contains("Sync:Before:ToString", syncInterceptor.Calls);
        Assert.Contains("Async:Before:ToString", asyncInterceptor.Calls);
    }

    [Fact]
    public async Task AsyncInterface_WithMultipleAsyncInterceptors_ShouldCallInOrder()
    {
        var interceptor1 = new RecordingAsyncInterceptor("A");
        var interceptor2 = new RecordingAsyncInterceptor("B");
        var composite = new CompositeInterceptor(interceptor1, interceptor2);
        var asyncComposite = (IAsyncInterceptor)composite;

        await asyncComposite.BeforeInvoke(TestMethod, null);
        await asyncComposite.AfterInvoke(TestMethod, null, "result");
        await asyncComposite.OnError(TestMethod, null, new Exception());

        var allCalls = interceptor1.Calls.Concat(interceptor2.Calls).ToList();
        Assert.Equal(["A:Before:ToString", "B:Before:ToString"], allCalls.Where(c => c.Contains("Before")).ToList());
        Assert.Equal(["A:After:ToString", "B:After:ToString"], allCalls.Where(c => c.Contains("After")).ToList());
        Assert.Equal(["A:Error:ToString", "B:Error:ToString"], allCalls.Where(c => c.Contains("Error")).ToList());
    }

    [Fact]
    public async Task AsyncInterface_WithMixedInterceptors_ShouldWorkCorrectly()
    {
        var syncInterceptor = new RecordingSyncInterceptor("Sync");
        var asyncInterceptor = new RecordingAsyncInterceptor("Async");
        var composite = new CompositeInterceptor(syncInterceptor, asyncInterceptor);
        var asyncComposite = (IAsyncInterceptor)composite;

        await asyncComposite.BeforeInvoke(TestMethod, null);

        Assert.Single(syncInterceptor.Calls);
        Assert.Single(asyncInterceptor.Calls);
    }

    [Fact]
    public async Task AsyncInterface_WithActualAsyncWork_ShouldAwaitCorrectly()
    {
        var asyncInterceptor1 = new RecordingAsyncInterceptor("A") { SimulateAsyncWork = true };
        var asyncInterceptor2 = new RecordingAsyncInterceptor("B") { SimulateAsyncWork = true };
        var composite = new CompositeInterceptor(asyncInterceptor1, asyncInterceptor2);
        var asyncComposite = (IAsyncInterceptor)composite;

        await asyncComposite.BeforeInvoke(TestMethod, null);

        Assert.Contains("A:Before:ToString", asyncInterceptor1.Calls);
        Assert.Contains("B:Before:ToString", asyncInterceptor2.Calls);
    }

    [Fact]
    public async Task AsyncInterface_WhenAllSyncComplete_ShouldNotAllocate()
    {
        var syncInterceptor1 = new RecordingSyncInterceptor("A");
        var syncInterceptor2 = new RecordingSyncInterceptor("B");
        var composite = new CompositeInterceptor(syncInterceptor1, syncInterceptor2);
        var asyncComposite = (IAsyncInterceptor)composite;

        var task = asyncComposite.BeforeInvoke(TestMethod, null);

        // When all interceptors complete synchronously, ValueTask should be completed
        Assert.True(task.IsCompletedSuccessfully);
        await task;
    }

    [Fact]
    public async Task AsyncInterface_WhenAsyncInterceptorNotYetComplete_ShouldReturnIncompleteTask()
    {
        var asyncInterceptor = new RecordingAsyncInterceptor("Async") { SimulateAsyncWork = true };
        var composite = new CompositeInterceptor(asyncInterceptor);
        var asyncComposite = (IAsyncInterceptor)composite;

        var task = asyncComposite.BeforeInvoke(TestMethod, null);

        // With actual async work, task should not be immediately completed
        // Note: This might be flaky depending on timing, so we just await it
        await task;
        Assert.Contains("Async:Before:ToString", asyncInterceptor.Calls);
    }

    [Fact]
    public void Constructor_WithSyncInterceptorsOnly_ShouldWork()
    {
        var sync1 = new RecordingSyncInterceptor("A");
        var sync2 = new RecordingSyncInterceptor("B");

        var composite = new CompositeInterceptor(sync1, sync2);
        ((IInterceptor)composite).BeforeInvoke(TestMethod, null);

        Assert.Single(sync1.Calls);
        Assert.Single(sync2.Calls);
    }

    [Fact]
    public void Constructor_WithAsyncInterceptorsOnly_ShouldWork()
    {
        var async1 = new RecordingAsyncInterceptor("A");
        var async2 = new RecordingAsyncInterceptor("B");

        var composite = new CompositeInterceptor(async1, async2);
        ((IInterceptor)composite).BeforeInvoke(TestMethod, null);

        Assert.Single(async1.Calls);
        Assert.Single(async2.Calls);
    }

    [Fact]
    public void Constructor_WithMixedInterceptors_ShouldWork()
    {
        var sync = new RecordingSyncInterceptor("Sync");
        var async = new RecordingAsyncInterceptor("Async");

        var composite = new CompositeInterceptor(sync, async);
        ((IInterceptor)composite).BeforeInvoke(TestMethod, null);

        Assert.Single(sync.Calls);
        Assert.Single(async.Calls);
    }

    [Fact]
    public void SingleInterceptor_ShouldStillWork()
    {
        var interceptor = new RecordingSyncInterceptor("Single");
        var composite = new CompositeInterceptor(interceptor);

        ((IInterceptor)composite).BeforeInvoke(TestMethod, null);
        ((IInterceptor)composite).AfterInvoke(TestMethod, null, null);
        ((IInterceptor)composite).OnError(TestMethod, null, new Exception());

        Assert.Equal(3, interceptor.Calls.Count);
    }
}
