using CoreMesh.Http.Responses;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CoreMesh.Http.Exceptions.Handlers;

/// <summary>
/// Handles <see cref="ValidationException"/> and writes a standardized validation error response.
/// </summary>
public sealed class ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger) : IExceptionHandler
{
    /// <summary>
    /// Attempts to handle the exception.
    /// </summary>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validation)
        {
            return false;
        }

        logger.LogWarning("Validation failed: {Message}", validation.Message);

        const int statusCode = StatusCodes.Status400BadRequest;
        httpContext.Response.StatusCode = statusCode;

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = "Validation failed",
            Detail = validation.Message,
            Instance = httpContext.Request.Path
        };
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
        problem.Extensions["errors"] = validation.Errors;

        await httpContext.Response.WriteAsJsonAsync(
            ApiResponse.OnFailure(problem, "validation_error"),
            cancellationToken);

        return true;
    }
}
