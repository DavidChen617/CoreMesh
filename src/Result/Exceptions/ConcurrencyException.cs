namespace CoreMesh.Result.Exceptions;

/// <summary>
/// Represents a concurrency conflict exception.
/// </summary>
/// <param name="message">The exception message.</param>
public sealed class ConcurrencyException(string message)
    : ConflictException(message);
