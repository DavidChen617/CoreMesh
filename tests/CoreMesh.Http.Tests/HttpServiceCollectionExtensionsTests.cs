using CoreMesh.Http.Exceptions.Handlers;
using CoreMesh.Http.Extensions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace CoreMesh.Http.Tests;

public sealed class HttpServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCoreMeshHttp_Should_Register_ExceptionHandlers()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCoreMeshHttp();

        using var provider = services.BuildServiceProvider();

        var handlers = provider.GetServices<IExceptionHandler>().ToList();

        Assert.Contains(handlers, x => x.GetType() == typeof(ValidationExceptionHandler));
        Assert.Contains(handlers, x => x.GetType() == typeof(ConflictExceptionHandler));
        Assert.Contains(handlers, x => x.GetType() == typeof(ForbiddenExceptionHandler));
        Assert.Contains(handlers, x => x.GetType() == typeof(NotFoundExceptionHandler));
        Assert.Contains(handlers, x => x.GetType() == typeof(GlobalExceptionHandler));
    }

    [Fact]
    public void AddCoreMeshHttp_Can_Be_Called_Multiple_Times()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        
        services.AddCoreMeshHttp();
        services.AddCoreMeshHttp();

        using var provider = services.BuildServiceProvider();

        var handlers = provider.GetServices<IExceptionHandler>().ToList();

        Assert.NotEmpty(handlers);
    }
}
