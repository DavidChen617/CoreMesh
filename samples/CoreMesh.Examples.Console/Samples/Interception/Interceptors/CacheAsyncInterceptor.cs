using System.Reflection;
using CoreMesh.Interception;

namespace CoreMesh.Examples.Console.Samples.Interception.Interceptors;

public class CacheAsyncInterceptor:  IAsyncInterceptor
{
    public ValueTask BeforeInvoke(MethodInfo method, object?[]? args)
    {
        System.Console.WriteLine("BeforeInvoke");
        return ValueTask.CompletedTask;
    }

    public ValueTask AfterInvoke(MethodInfo method, object?[]? args, object? result)
    {
        System.Console.WriteLine("AfterInvoke");
        return ValueTask.CompletedTask;
    }

    public ValueTask OnError(MethodInfo method, object?[]? args, Exception error)
    {
        System.Console.WriteLine("OnError");
        return ValueTask.CompletedTask;
    }
}
