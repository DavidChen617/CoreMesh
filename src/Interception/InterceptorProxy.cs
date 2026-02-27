using System.Collections.Concurrent;
using System.Reflection;

namespace CoreMesh.Interception;

/// <summary>
/// A dynamic proxy that wraps an interface implementation with synchronous interception support.
/// Uses <see cref="DispatchProxy"/> to intercept method calls and invoke <see cref="IInterceptor"/> hooks.
/// Supports Task, Task&lt;T&gt;, ValueTask, and ValueTask&lt;T&gt; return types.
/// </summary>
/// <typeparam name="T">The interface type to proxy.</typeparam>
public class InterceptorProxy<T> : DispatchProxy where T : class
{
    private T? _instance;
    private IInterceptor? _interceptor;
    private static readonly ConcurrentDictionary<Type, MethodInfo?> TaskMethodCache = new();
    private static readonly Type[] ParamTypes = [typeof(MethodInfo), typeof(object?[])];

    private static readonly MethodInfo InvokeAsyncTaskMethod = GetMethodInfo(nameof(InvokeAsyncTask));
    private static readonly MethodInfo InvokeAsyncGenericTask = GetMethodInfo(nameof(InvokeAsyncTask), 1);
    private static readonly MethodInfo InvokeAsyncValueTaskMethod = GetMethodInfo(nameof(InvokeAsyncValueTask));
    private static readonly MethodInfo InvokeAsyncGenericValueTask = GetMethodInfo(nameof(InvokeAsyncValueTask), 1);

    /// <inheritdoc />
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

        // Synchronous method invocation
        try
        {
            _interceptor?.BeforeInvoke(targetMethod, args);
            var result = targetMethod.Invoke(_instance, args);
            _interceptor?.AfterInvoke(targetMethod, args, result);
            return result;
        }
        catch (TargetInvocationException e) when (e.InnerException is not null)
        {
            _interceptor?.OnError(targetMethod, args, e.InnerException);
            throw;
        }
    }

    /// <summary>
    /// Creates a new proxy instance that wraps the target with the specified interceptor.
    /// </summary>
    /// <param name="target">The target instance to wrap.</param>
    /// <param name="interceptor">The interceptor to apply.</param>
    /// <returns>A proxy instance of type <typeparamref name="T"/>.</returns>
    public static T? Create(T target, IInterceptor interceptor)
    {
        var proxy = Create<T, InterceptorProxy<T>>() as InterceptorProxy<T>;
        proxy!._instance = target;
        proxy._interceptor = interceptor;
        return proxy as T;
    }

    private async Task InvokeAsyncTask(MethodInfo targetMethod, object?[]? args)
    {
        try
        {
            _interceptor?.BeforeInvoke(targetMethod, args);
            await (Task)targetMethod.Invoke(_instance, args)!;
            _interceptor?.AfterInvoke(targetMethod, args, null);
        }
        catch (Exception e)
        {
            _interceptor?.OnError(targetMethod, args, e);
            throw;
        }
    }

    private async Task<TResult> InvokeAsyncTask<TResult>(MethodInfo targetMethod, object?[]? args)
    {
        try
        {
            _interceptor?.BeforeInvoke(targetMethod, args);
            var result = await (Task<TResult>)targetMethod.Invoke(_instance, args)!;
            _interceptor?.AfterInvoke(targetMethod, args, result);
            return result;
        }
        catch (Exception e)
        {
            _interceptor?.OnError(targetMethod, args, e);
            throw;
        }
    }

    private async ValueTask InvokeAsyncValueTask(MethodInfo targetMethod, object?[]? args)
    {
        try
        {
            _interceptor?.BeforeInvoke(targetMethod, args);
            await (ValueTask)targetMethod.Invoke(_instance, args)!;
            _interceptor?.AfterInvoke(targetMethod, args, null);
        }
        catch (Exception e)
        {
            _interceptor?.OnError(targetMethod, args, e);
            throw;
        }
    }

    private async ValueTask<TResult> InvokeAsyncValueTask<TResult>(MethodInfo targetMethod, object?[]? args)
    {
        try
        {
            _interceptor?.BeforeInvoke(targetMethod, args);
            var result = await (ValueTask<TResult>)targetMethod.Invoke(_instance, args)!;
            _interceptor?.AfterInvoke(targetMethod, args, result);
            return result;
        }
        catch (Exception e)
        {
            _interceptor?.OnError(targetMethod, args, e);
            throw;
        }
    }

    private static MethodInfo GetMethodInfo(string methodName, int genericParameterCount = 0) =>
        typeof(InterceptorProxy<T>).GetMethod(
            methodName,
            genericParameterCount,
            BindingFlags.NonPublic | BindingFlags.Instance,
            binder: null,
            ParamTypes,
            modifiers: null)!;
}
