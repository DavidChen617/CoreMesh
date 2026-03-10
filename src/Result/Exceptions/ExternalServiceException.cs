using System.Net;

namespace CoreMesh.Result.Exceptions;

/// <summary>
/// Represents an external service exception (HTTP 502).
/// </summary>
/// <param name="message">The exception message.</param>
public sealed class ExternalServiceException(string message) :
    AppException(message, HttpStatusCode.BadGateway);
