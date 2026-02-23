using System.Net;

namespace CoreMesh.Http.Exceptions;

/// <summary>
/// Represents a forbidden exception (HTTP 403).
/// </summary>
/// <param name="message">The exception message.</param>
public sealed class ForbiddenException(string message) :
    AppException(message, HttpStatusCode.Forbidden);
