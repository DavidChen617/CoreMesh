using CoreMesh.Http.Responses;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace CoreMesh.Http.Exceptions.Handlers;

/// <summary>
/// Handles <see cref="ConflictException"/> and writes a standardized HTTP response.
/// </summary>
public sealed class ConflictExceptionHandler(ILogger<ConflictExceptionHandler> logger) : IExceptionHandler
{
    /// <summary>
    /// Attempts to handle the exception.
    /// </summary>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ConflictException conflict)
        {
            return false;
        }

        logger.LogWarning("Conflict occurred: {Message}", conflict.Message);

        const int statusCode = StatusCodes.Status409Conflict;
        httpContext.Response.StatusCode = statusCode;

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = ReasonPhrases.GetReasonPhrase(statusCode),
            Detail = conflict.Message,
            Instance = httpContext.Request.Path
        };
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        await httpContext.Response.WriteAsJsonAsync(
            ApiResponse.OnFailure(problem, "conflict"),
            cancellationToken);

        return true;
    }
}
