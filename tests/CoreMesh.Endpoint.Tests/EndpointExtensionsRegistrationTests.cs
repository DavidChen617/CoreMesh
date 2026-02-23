using CoreMesh.Endpoints;
using CoreMesh.Endpoints.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace CoreMesh.Endpoint.Tests;

public sealed class EndpointExtensionsRegistrationTests
{
    [Fact]
    public void AddEndpoints_Should_Register_IEndpoint()
    {
        var services = new ServiceCollection();

        services.AddEndpoints();

        using var provider = services.BuildServiceProvider();
        var endpoints = provider.GetServices<IEndpoint>().ToList();

        Assert.Contains(endpoints, x => x.GetType() == typeof(TestEndpoint));
    }

    [Fact]
    public void AddEndpoints_Should_Register_IGroupEndpoint()
    {
        var services = new ServiceCollection();

        services.AddEndpoints();

        using var provider = services.BuildServiceProvider();
        var groups = provider.GetServices<IGroupEndpoint>().ToList();

        Assert.Contains(groups, x => x.GetType() == typeof(TestGroupEndpoint));
    }

    [Fact]
    public void AddEndpoints_Should_Register_IGroupedEndpoint()
    {
        var services = new ServiceCollection();

        services.AddEndpoints();

        using var provider = services.BuildServiceProvider();
        var groupedEndpoints = provider.GetServices<IGroupedEndpoint>().ToList();

        Assert.Contains(groupedEndpoints, x => x.GetType() == typeof(TestGroupedEndpoint));
    }

    private sealed class TestEndpoint : IEndpoint
    {
        public void AddRoute(IEndpointRouteBuilder app)
        {
        }
    }

    private sealed class TestGroupEndpoint : IGroupEndpoint
    {
        public string GroupPrefix => "/test";

        public void Configure(RouteGroupBuilder group)
        {
        }
    }

    private sealed class TestGroupedEndpoint : IGroupedEndpoint<TestGroupEndpoint>
    {
        public void AddRoute(RouteGroupBuilder group)
        {
        }
    }
}
