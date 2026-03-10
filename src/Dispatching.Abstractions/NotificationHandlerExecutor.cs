namespace CoreMesh.Dispatching.Abstractions;

/// <summary>
/// Wraps a notification handler instance and its invocation callback.
/// </summary>
/// <param name="HandlerInstance">The handler instance.</param>
/// <param name="HandlerCallback">The callback to invoke the handler.</param>
public record NotificationHandlerExecutor(
    object HandlerInstance,
    Func<INotification, CancellationToken, Task> HandlerCallback);
