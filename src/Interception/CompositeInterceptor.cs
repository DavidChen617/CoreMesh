using System.Reflection;

namespace CoreMesh.Interception;

public class CompositeInterceptor : IInterceptor, IAsyncInterceptor
{
    private readonly IInterceptorBase[] _interceptors;

    public CompositeInterceptor(params IInterceptorBase[] interceptors)
        => _interceptors = interceptors;

    public CompositeInterceptor(params IInterceptor[] interceptors)
        : this(interceptors.Cast<IInterceptorBase>().ToArray())
    {
    }

    public CompositeInterceptor(params IAsyncInterceptor[] interceptors)
        : this(interceptors.Cast<IInterceptorBase>().ToArray())
    {
    }

    void IInterceptor.BeforeInvoke(MethodInfo method, object?[]? args)
    {
        foreach (var interceptor in _interceptors)
            if (interceptor is IInterceptor sync)
                sync.BeforeInvoke(method, args);
            else if (interceptor is IAsyncInterceptor async)
                async.BeforeInvoke(method, args).GetAwaiter().GetResult();
    }

    void IInterceptor.AfterInvoke(MethodInfo method, object?[]? args, object? result)
    {
        foreach (var interceptor in _interceptors)
            if (interceptor is IInterceptor sync)
                sync.AfterInvoke(method, args, result);
            else if (interceptor is IAsyncInterceptor async)
                async.AfterInvoke(method, args, result).GetAwaiter().GetResult();
    }

    void IInterceptor.OnError(MethodInfo method, object?[]? args, Exception error)
    {
        foreach (var interceptor in _interceptors)
            if (interceptor is IInterceptor sync)
                sync.OnError(method, args, error);
            else if (interceptor is IAsyncInterceptor async)
                async.OnError(method, args, error).GetAwaiter().GetResult();
    }

    ValueTask IAsyncInterceptor.BeforeInvoke(MethodInfo method, object?[]? args)
    {
        for (var i = 0; i < _interceptors.Length; i++)
        {
            var task = InvokeBeforeOne(_interceptors[i], method, args);
            if (!task.IsCompletedSuccessfully)
                return BeforeInvokeAsync(task, method, args, i + 1);
        }

        return default;
    }

    ValueTask IAsyncInterceptor.AfterInvoke(MethodInfo method, object?[]? args, object? result)
    {
        for (var i = 0; i < _interceptors.Length; i++)
        {
            var task = InvokeAfterOne(_interceptors[i], method, args, result);
            if (!task.IsCompletedSuccessfully)
                return AfterInvokeAsync(task, method, args, result, i + 1);
        }

        return default;
    }

    ValueTask IAsyncInterceptor.OnError(MethodInfo method, object?[]? args, Exception error)
    {
        for (var i = 0; i < _interceptors.Length; i++)
        {
            var task = InvokeErrorOne(_interceptors[i], method, args, error);
            if (!task.IsCompletedSuccessfully)
                return OnErrorAsync(task, method, args, error, i + 1);
        }

        return default;
    }

    private static ValueTask InvokeBeforeOne(IInterceptorBase interceptor, MethodInfo method, object?[]? args)
    {
        if (interceptor is IAsyncInterceptor async)
            return async.BeforeInvoke(method, args);

        ((IInterceptor)interceptor).BeforeInvoke(method, args);
        return default;
    }

    private static ValueTask InvokeAfterOne(IInterceptorBase interceptor, MethodInfo method, object?[]? args,
        object? result)
    {
        if (interceptor is IAsyncInterceptor async)
            return async.AfterInvoke(method, args, result);

        ((IInterceptor)interceptor).AfterInvoke(method, args, result);
        return default;
    }

    private static ValueTask InvokeErrorOne(IInterceptorBase interceptor, MethodInfo method, object?[]? args,
        Exception error)
    {
        if (interceptor is IAsyncInterceptor async)
            return async.OnError(method, args, error);

        ((IInterceptor)interceptor).OnError(method, args, error);
        return default;
    }

    private async ValueTask BeforeInvokeAsync(ValueTask current, MethodInfo method, object?[]? args, int startIndex)
    {
        await current;
        for (var i = startIndex; i < _interceptors.Length; i++)
            await InvokeBeforeOne(_interceptors[i], method, args);
    }

    private async ValueTask AfterInvokeAsync(ValueTask current, MethodInfo method, object?[]? args, object? result,
        int startIndex)
    {
        await current;
        for (var i = startIndex; i < _interceptors.Length; i++)
            await InvokeAfterOne(_interceptors[i], method, args, result);
    }

    private async ValueTask OnErrorAsync(ValueTask current, MethodInfo method, object?[]? args, Exception error,
        int startIndex)
    {
        await current;
        for (var i = startIndex; i < _interceptors.Length; i++)
            await InvokeErrorOne(_interceptors[i], method, args, error);
    }
}
