using CoreMesh.Http.Responses;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace CoreMesh.Http.Exceptions.Handlers;

/// <summary>
/// Handles <see cref="NotFoundException"/> and writes a standardized HTTP response.
/// </summary>
public sealed class NotFoundExceptionHandler(ILogger<NotFoundExceptionHandler> logger) : IExceptionHandler
{
    /// <summary>
    /// Attempts to handle the exception.
    /// </summary>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not NotFoundException notFoundEx)
        {
            return false;
        }

        logger.LogWarning("Resource not found: {Message}", notFoundEx.Message);

        const int statusCode = StatusCodes.Status404NotFound;
        httpContext.Response.StatusCode = statusCode;

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = ReasonPhrases.GetReasonPhrase(statusCode),
            Detail = notFoundEx.Message,
            Instance = httpContext.Request.Path
        };
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        await httpContext.Response.WriteAsJsonAsync(
            ApiResponse.OnFailure(problem, "not_found"),
            cancellationToken);

        return true;
    }
}
