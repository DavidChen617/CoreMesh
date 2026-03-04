using CoreMesh.Endpoints;
using CoreMesh.Endpoints.Extensions;
using Microsoft.AspNetCore.Builder;

namespace CoreMesh.Endpoint.Tests;

public sealed class EndpointExtensionsMappingTests
{
    [Fact]
    public void MapEndpoints_Should_Invoke_Root_Endpoint_AddRoute()
    {
        TestRootEndpoint.Reset();

        var builder = WebApplication.CreateBuilder();
        builder.Services.AddEndpoints([typeof(EndpointExtensionsMappingTests).Assembly]);

        var app = builder.Build();

        app.MapEndpoints();

        Assert.True(TestRootEndpoint.AddRouteCalled);
    }

    [Fact]
    public void MapEndpoints_Should_Invoke_Group_And_Grouped_Endpoint_AddRoute()
    {
        TestMapGroupEndpoint.Reset();
        TestGroupedMapEndpoint.Reset();

        var builder = WebApplication.CreateBuilder();
        builder.Services.AddEndpoints([typeof(EndpointExtensionsMappingTests).Assembly]);
        var app = builder.Build();

        app.MapEndpoints();

        Assert.True(TestMapGroupEndpoint.ConfigureCalled);
        Assert.True(TestGroupedMapEndpoint.AddRouteCalled);
    }

    [Fact]
    public void MapEndpoints_Should_Not_Map_GroupedEndpoint_To_Wrong_Group()
    {
        TestMapGroupEndpoint.Reset();
        AnotherTestMapGroupEndpoint.Reset();
        TestGroupedMapEndpoint.Reset();
        WrongGroupGroupedEndpoint.Reset();

        var builder = WebApplication.CreateBuilder();
        builder.Services.AddEndpoints([typeof(EndpointExtensionsMappingTests).Assembly]);

        var app = builder.Build();

        app.MapEndpoints();

        Assert.True(TestMapGroupEndpoint.ConfigureCalled);
        Assert.True(AnotherTestMapGroupEndpoint.ConfigureCalled);

        Assert.True(TestGroupedMapEndpoint.AddRouteCalled);
        Assert.True(WrongGroupGroupedEndpoint.AddRouteCalled);

        Assert.Same(TestMapGroupEndpoint.GroupInstance, TestGroupedMapEndpoint.MappedGroup);
        Assert.Same(AnotherTestMapGroupEndpoint.GroupInstance, WrongGroupGroupedEndpoint.MappedGroup);
        Assert.NotSame(AnotherTestMapGroupEndpoint.GroupInstance, TestGroupedMapEndpoint.MappedGroup);
        Assert.NotSame(TestMapGroupEndpoint.GroupInstance, WrongGroupGroupedEndpoint.MappedGroup);
    }

    private sealed class TestRootEndpoint : IEndpoint
    {
        public static bool AddRouteCalled { get; private set; }

        public static void Reset() => AddRouteCalled = false;

        public void AddRoute(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder app)
        {
            AddRouteCalled = true;
        }
    }

    private sealed class TestMapGroupEndpoint : IGroupEndpoint
    {
        public static bool ConfigureCalled { get; private set; }
        public static Microsoft.AspNetCore.Routing.RouteGroupBuilder? GroupInstance { get; private set; }

        public string GroupPrefix => "/mapping-tests";

        public static void Reset()
        {
            ConfigureCalled = false;
            GroupInstance = null;
        }

        public void Configure(Microsoft.AspNetCore.Routing.RouteGroupBuilder group)
        {
            ConfigureCalled = true;
            GroupInstance = group;
        }
    }

    private sealed class TestGroupedMapEndpoint : IGroupedEndpoint<TestMapGroupEndpoint>
    {
        public static bool AddRouteCalled { get; private set; }
        public static Microsoft.AspNetCore.Routing.RouteGroupBuilder? MappedGroup { get; private set; }

        public static void Reset()
        {
            AddRouteCalled = false;
            MappedGroup = null;
        }

        public void AddRoute(Microsoft.AspNetCore.Routing.RouteGroupBuilder group)
        {
            AddRouteCalled = true;
            MappedGroup = group;
        }
    }

    private sealed class AnotherTestMapGroupEndpoint : IGroupEndpoint
    {
        public static bool ConfigureCalled { get; private set; }
        public static Microsoft.AspNetCore.Routing.RouteGroupBuilder? GroupInstance { get; private set; }

        public string GroupPrefix => "/mapping-tests-2";

        public static void Reset()
        {
            ConfigureCalled = false;
            GroupInstance = null;
        }

        public void Configure(Microsoft.AspNetCore.Routing.RouteGroupBuilder group)
        {
            ConfigureCalled = true;
            GroupInstance = group;
        }
    }

    private sealed class WrongGroupGroupedEndpoint : IGroupedEndpoint<AnotherTestMapGroupEndpoint>
    {
        public static bool AddRouteCalled { get; private set; }
        public static Microsoft.AspNetCore.Routing.RouteGroupBuilder? MappedGroup { get; private set; }

        public static void Reset()
        {
            AddRouteCalled = false;
            MappedGroup = null;
        }

        public void AddRoute(Microsoft.AspNetCore.Routing.RouteGroupBuilder group)
        {
            AddRouteCalled = true;
            MappedGroup = group;
        }
    }
}
