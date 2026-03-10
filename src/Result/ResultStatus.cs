namespace CoreMesh.Result;

/// <summary>
/// Defines the possible outcome statuses of a <see cref="Result"/> or <see cref="Result{T}"/> operation.
/// </summary>
public enum ResultStatus
{
    /// <summary>
    /// The operation completed successfully and returned a payload. Maps to HTTP 200 OK.
    /// </summary>
    Ok,

    /// <summary>
    /// The operation successfully created a new resource. Maps to HTTP 201 Created.
    /// </summary>
    Created,

    /// <summary>
    /// The operation succeeded but returns no content. Maps to HTTP 204 No Content.
    /// </summary>
    NoContent,

    /// <summary>
    /// The request was malformed or contained invalid arguments. Maps to HTTP 400 Bad Request.
    /// </summary>
    BadRequest,

    /// <summary>
    /// The caller is not authorized to perform the requested operation. Maps to HTTP 403 Forbidden.
    /// </summary>
    Forbidden,

    /// <summary>
    /// One or more validation rules were violated. Maps to HTTP 422 Unprocessable Entity.
    /// </summary>
    Invalid,

    /// <summary>
    /// The requested resource could not be found. Maps to HTTP 404 Not Found.
    /// </summary>
    NotFound,
}
