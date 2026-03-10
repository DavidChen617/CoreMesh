using System.Net;

namespace CoreMesh.Result.Exceptions;

/// <summary>
/// Represents a conflict exception (HTTP 409).
/// </summary>
/// <param name="message">The exception message.</param>
public class ConflictException(string message) :
    AppException(message, HttpStatusCode.Conflict);
