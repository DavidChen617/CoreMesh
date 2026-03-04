using CoreMesh.Http.Exceptions;
using CoreMesh.Http.Exceptions.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;

namespace CoreMesh.Http.Tests;

public sealed class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_Should_Map_AppException_Status_And_Code()
    {
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);

        var context = CreateHttpContext(isDevelopment: false);
        var exception = new NotFoundException("Product", 100);

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);

        var json = await ReadBodyAsync(context);
        Assert.Contains("\"code\":\"not_found\"", json);
        Assert.Contains("\"status\":404", json);
        Assert.Contains("\"success\":false", json);
    }

    [Fact]
    public async Task TryHandleAsync_Should_Map_UnauthorizedAccessException()
    {
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);

        var context = CreateHttpContext(isDevelopment: false);

        var handled = await handler.TryHandleAsync(
            context,
            new UnauthorizedAccessException("Forbidden area"),
            CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);

        var json = await ReadBodyAsync(context);
        Assert.Contains("\"code\":\"unauthorized\"", json);
        Assert.Contains("\"status\":401", json);
    }

    [Fact]
    public async Task TryHandleAsync_Should_Hide_Unexpected_Error_Message_In_Production()
    {
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);

        var context = CreateHttpContext(isDevelopment: false);

        var handled = await handler.TryHandleAsync(
            context,
            new Exception("sensitive internal message"),
            CancellationToken.None);

        Assert.True(handled);

        var json = await ReadBodyAsync(context);
        Assert.Contains("An unexpected error occurred", json);
        Assert.DoesNotContain("sensitive internal message", json);
    }

    [Fact]
    public async Task TryHandleAsync_Should_Show_Error_Message_In_Development()
    {
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);

        var context = CreateHttpContext(isDevelopment: true);

        var handled = await handler.TryHandleAsync(
            context,
            new Exception("dev-visible message"),
            CancellationToken.None);

        Assert.True(handled);

        var json = await ReadBodyAsync(context);
        Assert.Contains("dev-visible message", json);
    }

    private static DefaultHttpContext CreateHttpContext(bool isDevelopment)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IHostEnvironment>(new FakeHostEnvironment
        {
            EnvironmentName = isDevelopment ? Environments.Development : Environments.Production
        });

        var provider = services.BuildServiceProvider();

        var context = new DefaultHttpContext
        {
            RequestServices = provider,
            TraceIdentifier = "trace-xyz"
        };

        context.Response.Body = new MemoryStream();
        context.Request.Path = "/test";

        return context;
    }

    private static async Task<string> ReadBodyAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        return await new StreamReader(context.Response.Body).ReadToEndAsync();
    }

    private sealed class FakeHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ApplicationName { get; set; } = "CoreMesh.Http.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = default!;
    }
}
