// using System.ComponentModel.DataAnnotations;

using CoreMesh.Http.Exceptions;
using CoreMesh.Http.Exceptions.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace CoreMesh.Http.Tests;

public sealed class ValidationExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_Should_Write_ApiResponse_With_Problem_Extensions_Errors()
    {
        var handler = new ValidationExceptionHandler(NullLogger<ValidationExceptionHandler>.Instance);

        var context = new DefaultHttpContext();
        context.Request.Path = "/products";
        context.Response.Body = new MemoryStream();
        context.TraceIdentifier = "trace-123";

        var exception = new ValidationException(new Dictionary<string, string[]>
        {
            ["Name"] = ["Name is required."]
        });

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);

        context.Response.Body.Position = 0;
        var json = await new StreamReader(context.Response.Body).ReadToEndAsync();

        Assert.Contains("\"success\":false", json);
        Assert.Contains("\"code\":\"validation_error\"", json);
        Assert.Contains("\"traceId\":\"trace-123\"", json);
        Assert.Contains("\"problem\":", json);
        Assert.Contains("\"errors\":", json);
        Assert.Contains("\"Name\"", json);
        Assert.Contains("Name is required.", json);
    }

    [Fact]
    public async Task TryHandleAsync_Should_Return_False_For_Non_ValidationException()
    {
        var handler = new ValidationExceptionHandler(NullLogger<ValidationExceptionHandler>.Instance);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var handled = await handler.TryHandleAsync(
            context,
            new InvalidOperationException("boom"),
            CancellationToken.None);

        Assert.False(handled);
    }
}
