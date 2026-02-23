using CoreMesh.Http.Responses;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CoreMesh.Http.Exceptions.Handlers;

/// <summary>
/// Handles unhandled exceptions and writes a fallback standardized HTTP response.
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    /// <summary>
    /// Attempts to handle the exception.
    /// </summary>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception occurred. TraceId: {TraceId}", httpContext.TraceIdentifier);

        var statusCode = MapStatusCode(exception);
        var message = GetSafeErrorMessage(exception, httpContext);

        httpContext.Response.StatusCode = statusCode;

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = ReasonPhrases.GetReasonPhrase(statusCode),
            Detail = message,
            Instance = httpContext.Request.Path
        };
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        await httpContext.Response.WriteAsJsonAsync(
            ApiResponse.OnFailure(problem, MapCode(exception)),
            cancellationToken);

        return true;
    }

    private static int MapStatusCode(Exception exception)
    {
        return exception switch
        {
            AppException appEx => (int)appEx.StatusCode,
            ArgumentNullException => StatusCodes.Status400BadRequest,
            ArgumentException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static string MapCode(Exception exception)
    {
        return exception switch
        {
            BadRequestException => "bad_request",
            ValidationException => "validation_error",
            ConcurrencyException => "concurrency_conflict",
            ConflictException => "conflict",
            ForbiddenException => "forbidden",
            NotFoundException => "not_found",
            ExternalServiceException => "external_service_error",
            UnauthorizedAccessException => "unauthorized",
            ArgumentNullException => "bad_request",
            ArgumentException => "bad_request",
            _ => "unexpected_error"
        };
    }

    private static string GetSafeErrorMessage(Exception exception, HttpContext context)
    {
        var env = context.RequestServices.GetRequiredService<IHostEnvironment>();

        if (env.IsDevelopment())
        {
            return exception.Message;
        }

        return exception is AppException ? exception.Message : "An unexpected error occurred";
    }
}
