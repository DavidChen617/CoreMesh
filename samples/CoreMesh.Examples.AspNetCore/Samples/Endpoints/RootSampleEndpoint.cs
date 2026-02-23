using CoreMesh.Dispatching;
using CoreMesh.Endpoints;
using CoreMesh.Examples.AspNetCore.Samples.Queries;
using CoreMesh.Http.Responses;

namespace CoreMesh.Examples.AspNetCore.Samples.Endpoints;

public sealed class RootSampleEndpoint : IEndpoint
{
    public void AddRoute(IEndpointRouteBuilder app)
    {
        app.MapGet("/",
            async (IDispatcher dispatcher) =>
                TypedResults.Ok(
                    ApiResponse<SampleResponse>.OnSuccess(
                        await dispatcher.Send(new SampleQuery("Foo", "Bar")))));
    }
}
