using System.Net.Mime;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CoreMesh.Middleware.Idempotency;

public class IdempotencyMiddleware(
    RequestDelegate next,
    ILogger<IdempotencyMiddleware> logger,
    IServiceScopeFactory scopeFactory,
    IdempotencyOptions options)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var idempotencyInfo = GetIdempotencyAttribute(context);

        if (idempotencyInfo is null || !HttpMethods.IsPost(context.Request.Method))
        {
            await next(context);
            return;
        }

        var headerKeyName = idempotencyInfo.CustomHeaderName ?? options.IdempotencyKeyName;

        if (!context.Request.Headers.TryGetValue(headerKeyName, out var idempotencyKey) ||
            string.IsNullOrWhiteSpace(idempotencyKey))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = MediaTypeNames.Application.Json;
            var errorMessage = $"The {headerKeyName} header is required.";

            var error = options.ErrorResponseFormatter?.Invoke(errorMessage, context) ??
                        JsonSerializer.Serialize(new { error = errorMessage });
            await context.Response.WriteAsync(error);

            return;
        }

        IdempotencyResult? existing;
        await using (var scope = scopeFactory.CreateAsyncScope())
        {
            var handler = scope.ServiceProvider.GetRequiredService<IIdempotencyHandler>();
            existing = await handler.GetExistingResponseAsync(idempotencyKey!, context.RequestAborted);
        }

        if (existing is not null)
        {
            logger.LogInformation("Replaying cached response for {KeyName}: {Key}", options.IdempotencyKeyName, idempotencyKey);
            context.Response.StatusCode = existing.StatusCode;
            context.Response.ContentType = MediaTypeNames.Application.Json;
            context.Response.Headers[options.IdempotencyHeaderReplayed] = "true";
            await context.Response.WriteAsync(existing.Payload);
            return;
        }

        var originBody = context.Response.Body;
        using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        try
        {
            await next(context);
            buffer.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(buffer).ReadToEndAsync();

            if (context.Response.StatusCode is >= 200 and < 300)
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var handler = scope.ServiceProvider.GetRequiredService<IIdempotencyHandler>();
                await handler.StoreResponseAsync(idempotencyKey!, context.Response.StatusCode, responseBody, context.RequestAborted);
            }

            buffer.Seek(0, SeekOrigin.Begin);
            await buffer.CopyToAsync(originBody);
        }
        finally
        {
            context.Response.Body = originBody;
        }
    }

    private static IdempotencyInfo? GetIdempotencyAttribute(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint is null)
            return null;

        // endpoint metadata (covers Minimal API lambda attributes + Controller action attributes)
        var attribute = endpoint.Metadata.OfType<IdempotencyAttribute>().FirstOrDefault();
        if (attribute is not null)
            return new IdempotencyInfo(attribute.CustomIdempotencyKeyName);

        // controller method fallback
        var actionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
        if (actionDescriptor is not null)
        {
            attribute = actionDescriptor.MethodInfo.GetCustomAttribute<IdempotencyAttribute>();
            if (attribute is not null)
                return new IdempotencyInfo(attribute.CustomIdempotencyKeyName);
        }

        // Minimal API method fallback
        var methodInfo = endpoint.Metadata.GetMetadata<MethodInfo>();
        if (methodInfo is not null)
        {
            attribute = methodInfo.GetCustomAttribute<IdempotencyAttribute>();
            if (attribute is not null)
                return new IdempotencyInfo(attribute.CustomIdempotencyKeyName);
        }
        
        return null;
    }

    private record IdempotencyInfo(string? CustomHeaderName);
}
