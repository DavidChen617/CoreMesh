using CoreMesh.Http.Responses;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace CoreMesh.Http.Exceptions.Handlers;

/// <summary>
/// Handles <see cref="ForbiddenException"/> and writes a standardized HTTP response.
/// </summary>
public sealed class ForbiddenExceptionHandler(ILogger<ForbiddenExceptionHandler> logger) : IExceptionHandler
{
    /// <summary>
    /// Attempts to handle the exception.
    /// </summary>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ForbiddenException forbidden)
        {
            return false;
        }

        logger.LogWarning("Forbidden: {Message}", forbidden.Message);

        const int statusCode = StatusCodes.Status403Forbidden;
        httpContext.Response.StatusCode = statusCode;

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = ReasonPhrases.GetReasonPhrase(statusCode),
            Detail = forbidden.Message,
            Instance = httpContext.Request.Path
        };
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        await httpContext.Response.WriteAsJsonAsync(
            ApiResponse.OnFailure(problem, "forbidden"),
            cancellationToken);

        return true;
    }
}
