using System.Collections.Concurrent;
using System.Reflection;

namespace CoreMesh.Interception;

/// <summary>
/// Async interceptor proxy using IAsyncInterceptor
/// </summary>
public class AsyncInterceptorProxy<T> : DispatchProxy where T : class
{
    private T? _instance;
    private IAsyncInterceptor? _interceptor;
    private static readonly ConcurrentDictionary<Type, MethodInfo?> TaskMethodCache = new();
    private static readonly Type[] ParamTypes = [typeof(MethodInfo), typeof(object?[])];

    private static readonly MethodInfo InvokeAsyncTaskMethod = GetMethodInfo(nameof(InvokeAsyncTask));
    private static readonly MethodInfo InvokeAsyncGenericTask = GetMethodInfo(nameof(InvokeAsyncTask), 1);
    private static readonly MethodInfo InvokeAsyncValueTaskMethod = GetMethodInfo(nameof(InvokeAsyncValueTask));
    private static readonly MethodInfo InvokeAsyncGenericValueTask = GetMethodInfo(nameof(InvokeAsyncValueTask), 1);

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        if (targetMethod is null)
            return null;

        var returnType = targetMethod.ReturnType;

        // Task or Task<T>
        if (typeof(Task).IsAssignableFrom(returnType))
        {
            if (!returnType.IsGenericType)
                return InvokeAsyncTaskMethod.Invoke(this, [targetMethod, args]);

            var method = TaskMethodCache.GetOrAdd(returnType, rt =>
                InvokeAsyncGenericTask.MakeGenericMethod(rt.GetGenericArguments()[0]));

            return method?.Invoke(this, [targetMethod, args]);
        }

        // ValueTask
        if (returnType == typeof(ValueTask))
            return InvokeAsyncValueTaskMethod.Invoke(this, [targetMethod, args]);

        // ValueTask<T>
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
        {
            var method = TaskMethodCache.GetOrAdd(returnType, rt =>
                InvokeAsyncGenericValueTask.MakeGenericMethod(rt.GetGenericArguments()[0]));

            return method?.Invoke(this, [targetMethod, args]);
        }

        // Sync method: blocking wait for async interceptor
        try
        {
            AwaitIfNeeded(_interceptor?.BeforeInvoke(targetMethod, args));
            var result = targetMethod.Invoke(_instance, args);
            AwaitIfNeeded(_interceptor?.AfterInvoke(targetMethod, args, result));
            return result;
        }
        catch (TargetInvocationException e) when (e.InnerException is not null)
        {
            AwaitIfNeeded(_interceptor?.OnError(targetMethod, args, e.InnerException));
            throw e.InnerException;
        }
    }

    public static T? Create(T target, IAsyncInterceptor interceptor)
    {
        var proxy = Create<T, AsyncInterceptorProxy<T>>() as AsyncInterceptorProxy<T>;
        proxy!._instance = target;
        proxy._interceptor = interceptor;
        return proxy as T;
    }

    private async Task InvokeAsyncTask(MethodInfo targetMethod, object?[]? args)
    {
        try
        {
            await (_interceptor?.BeforeInvoke(targetMethod, args) ?? ValueTask.CompletedTask);
            await (Task)targetMethod.Invoke(_instance, args)!;
            await (_interceptor?.AfterInvoke(targetMethod, args, null) ?? ValueTask.CompletedTask);
        }
        catch (Exception e)
        {
            await (_interceptor?.OnError(targetMethod, args, e) ?? ValueTask.CompletedTask);
            throw;
        }
    }

    private async Task<TResult> InvokeAsyncTask<TResult>(MethodInfo targetMethod, object?[]? args)
    {
        try
        {
            await (_interceptor?.BeforeInvoke(targetMethod, args) ?? ValueTask.CompletedTask);
            var result = await (Task<TResult>)targetMethod.Invoke(_instance, args)!;
            await (_interceptor?.AfterInvoke(targetMethod, args, result) ?? ValueTask.CompletedTask);
            return result;
        }
        catch (Exception e)
        {
            await (_interceptor?.OnError(targetMethod, args, e) ?? ValueTask.CompletedTask);
            throw;
        }
    }

    private async ValueTask InvokeAsyncValueTask(MethodInfo targetMethod, object?[]? args)
    {
        try
        {
            await (_interceptor?.BeforeInvoke(targetMethod, args) ?? ValueTask.CompletedTask);
            await (ValueTask)targetMethod.Invoke(_instance, args)!;
            await (_interceptor?.AfterInvoke(targetMethod, args, null) ?? ValueTask.CompletedTask);
        }
        catch (Exception e)
        {
            await (_interceptor?.OnError(targetMethod, args, e) ?? ValueTask.CompletedTask);
            throw;
        }
    }

    private async ValueTask<TResult> InvokeAsyncValueTask<TResult>(MethodInfo targetMethod, object?[]? args)
    {
        try
        {
            await (_interceptor?.BeforeInvoke(targetMethod, args) ?? ValueTask.CompletedTask);
            var result = await (ValueTask<TResult>)targetMethod.Invoke(_instance, args)!;
            await (_interceptor?.AfterInvoke(targetMethod, args, result) ?? ValueTask.CompletedTask);
            return result;
        }
        catch (Exception e)
        {
            await (_interceptor?.OnError(targetMethod, args, e) ?? ValueTask.CompletedTask);
            throw;
        }
    }

    private static void AwaitIfNeeded(ValueTask? task)
    {
        if (task is not { IsCompletedSuccessfully: false } t) return;
        t.AsTask().GetAwaiter().GetResult();
    }

    private static MethodInfo GetMethodInfo(string methodName, int genericParameterCount = 0) =>
        typeof(AsyncInterceptorProxy<T>).GetMethod(
            methodName,
            genericParameterCount,
            BindingFlags.NonPublic | BindingFlags.Instance,
            binder: null,
            ParamTypes,
            modifiers: null)!;
}
