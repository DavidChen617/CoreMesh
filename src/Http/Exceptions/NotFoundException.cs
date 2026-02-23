using System.Net;

namespace CoreMesh.Http.Exceptions;

/// <summary>
/// Represents a not-found exception (HTTP 404).
/// </summary>
/// <param name="resourceName">The missing resource name.</param>
/// <param name="key">The resource identifier.</param>
public sealed class NotFoundException(string resourceName, object key) :
    AppException($"{resourceName} with identifier '{key}' was not found.", HttpStatusCode.NotFound);
