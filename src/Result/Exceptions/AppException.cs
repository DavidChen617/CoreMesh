using System.Net;

namespace CoreMesh.Result.Exceptions;

/// <summary>
/// Represents the base exception type for HTTP-aware application exceptions.
/// </summary>
/// <param name="message">The exception message.</param>
/// <param name="statusCode">The HTTP status code associated with the exception.</param>
/// <param name="code">The application-specific error code.</param>
public abstract class AppException(
    string message,
    HttpStatusCode statusCode = HttpStatusCode.InternalServerError,
    string code = "unexpected_error")
    : Exception(message)
{
    /// <summary>
    /// Gets the HTTP status code associated with the exception.
    /// </summary>
    public HttpStatusCode StatusCode { get; } = statusCode;

    /// <summary>
    /// Gets the application-specific error code (e.g. "not_found", "conflict").
    /// </summary>
    public string Code { get; } = code;
}
