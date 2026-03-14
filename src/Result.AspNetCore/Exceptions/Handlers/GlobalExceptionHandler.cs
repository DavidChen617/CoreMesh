using CoreMesh.Result.Exceptions;
using CoreMesh.Result.Http;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CoreMesh.Result.Exceptions.Handlers;

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

        var statusCode = exception is AppException appEx ? (int)appEx.StatusCode : StatusCodes.Status500InternalServerError;
        var code = exception is AppException appException ? appException.Code : "unexpected_error";
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
            ApiResponse.OnFailure(problem, code),
            cancellationToken);

        return true;
    }

    private static string GetSafeErrorMessage(Exception exception, HttpContext context)
    {
        var env = context.RequestServices.GetRequiredService<IHostEnvironment>();

        if (env.IsDevelopment())
            return exception.Message;

        return exception is AppException ? exception.Message : "An unexpected error occurred";
    }
}
