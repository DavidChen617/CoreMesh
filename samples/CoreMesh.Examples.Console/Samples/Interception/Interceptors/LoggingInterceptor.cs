using System.Reflection;
using CoreMesh.Interception;

namespace CoreMesh.Examples.Console.Samples.Interception.Interceptors;

public class LoggingInterceptor : IInterceptor
{
    public void BeforeInvoke(MethodInfo method, object?[]? args)
    {
        System.Console.WriteLine("BeforeInvoke");
    }

    public void AfterInvoke(MethodInfo method, object?[]? args, object? result)
    {
        System.Console.WriteLine("AfterInvoke");
    }

    public void OnError(MethodInfo method, object?[]? args, Exception error)
    {
        System.Console.WriteLine("OnError");
    }
}
