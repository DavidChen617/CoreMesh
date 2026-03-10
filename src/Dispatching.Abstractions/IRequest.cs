namespace CoreMesh.Dispatching.Abstractions;

/// <summary>
/// Represents a request that does not return a response payload.
/// </summary>
public interface IRequest;

/// <summary>
/// Represents a request that returns a response payload.
/// </summary>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface IRequest<out TResponse>;
