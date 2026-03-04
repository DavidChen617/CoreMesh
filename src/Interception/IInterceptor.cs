using System.Reflection;

namespace CoreMesh.Interception;

/// <summary>
/// Base marker interface for all interceptor types.
/// </summary>
public interface IInterceptorBase;

/// <summary>
/// Synchronous interceptor interface for AOP method interception.
/// </summary>
public interface IInterceptor : IInterceptorBase
{
    /// <summary>
    /// Invoked before the target method is called.
    /// </summary>
    /// <param name="method">The method being invoked.</param>
    /// <param name="args">The arguments passed to the method.</param>
    void BeforeInvoke(MethodInfo method, object?[]? args);

    /// <summary>
    /// Invoked after the target method completes successfully.
    /// </summary>
    /// <param name="method">The method that was invoked.</param>
    /// <param name="args">The arguments passed to the method.</param>
    /// <param name="result">The return value of the method, or null for void methods.</param>
    void AfterInvoke(MethodInfo method, object?[]? args, object? result);

    /// <summary>
    /// Invoked when the target method throws an exception.
    /// </summary>
    /// <param name="method">The method that threw the exception.</param>
    /// <param name="args">The arguments passed to the method.</param>
    /// <param name="error">The exception that was thrown.</param>
    void OnError(MethodInfo method, object?[]? args, Exception error);
}

/// <summary>
/// Asynchronous interceptor interface for AOP method interception.
/// Use this when interceptor logic requires async operations.
/// </summary>
public interface IAsyncInterceptor : IInterceptorBase
{
    /// <summary>
    /// Invoked before the target method is called.
    /// </summary>
    /// <param name="method">The method being invoked.</param>
    /// <param name="args">The arguments passed to the method.</param>
    /// <returns>A <see cref="ValueTask"/> representing the async operation.</returns>
    ValueTask BeforeInvoke(MethodInfo method, object?[]? args);

    /// <summary>
    /// Invoked after the target method completes successfully.
    /// </summary>
    /// <param name="method">The method that was invoked.</param>
    /// <param name="args">The arguments passed to the method.</param>
    /// <param name="result">The return value of the method, or null for void methods.</param>
    /// <returns>A <see cref="ValueTask"/> representing the async operation.</returns>
    ValueTask AfterInvoke(MethodInfo method, object?[]? args, object? result);

    /// <summary>
    /// Invoked when the target method throws an exception.
    /// </summary>
    /// <param name="method">The method that threw the exception.</param>
    /// <param name="args">The arguments passed to the method.</param>
    /// <param name="error">The exception that was thrown.</param>
    /// <returns>A <see cref="ValueTask"/> representing the async operation.</returns>
    ValueTask OnError(MethodInfo method, object?[]? args, Exception error);
}
