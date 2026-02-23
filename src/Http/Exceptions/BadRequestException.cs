using System.Net;

namespace CoreMesh.Http.Exceptions;

/// <summary>
/// Represents a bad request exception (HTTP 400).
/// </summary>
/// <param name="message">The exception message.</param>
public sealed class BadRequestException(string message) :
    AppException(message, HttpStatusCode.BadRequest);
