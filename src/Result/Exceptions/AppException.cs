using System.Net;

namespace CoreMesh.Result.Exceptions;

/// <summary>
/// Represents the base exception type for HTTP-aware application exceptions.
/// </summary>
/// <param name="message">The exception message.</param>
/// <param name="statusCode">The HTTP status code associated with the exception.</param>
public abstract class AppException(
    string message,
    HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
    : Exception(message)
{
    /// <summary>
    /// Gets the HTTP status code associated with the exception.
    /// </summary>
    public HttpStatusCode StatusCode { get; } = statusCode;
}
