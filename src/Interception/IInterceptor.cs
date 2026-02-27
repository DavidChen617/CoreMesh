using System.Reflection;

namespace CoreMesh.Interception;

public interface IInterceptorBase { }

public interface IInterceptor: IInterceptorBase
{
    void BeforeInvoke(MethodInfo method, object?[]? args);
    void AfterInvoke(MethodInfo method, object?[]? args, object? result);
    void OnError(MethodInfo method, object?[]? args,  Exception error);
}

public interface IAsyncInterceptor: IInterceptorBase
{
    ValueTask BeforeInvoke(MethodInfo method, object?[]? args);
    ValueTask AfterInvoke(MethodInfo method, object?[]? args, object? result);
    ValueTask OnError(MethodInfo method, object?[]? args,  Exception error);
}
