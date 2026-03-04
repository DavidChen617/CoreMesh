using CoreMesh.Dispatching;
using CoreMesh.Endpoints;
using CoreMesh.Examples.AspNetCore.Samples.Queries;
using CoreMesh.Http.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CoreMesh.Examples.AspNetCore.Samples.Endpoints;

public sealed class RootSampleEndpoint : IEndpoint
{
    public void AddRoute(IEndpointRouteBuilder app)
    {
        app.MapGet("/", HandleAsync);
    }

    private static async Task<IResult> HandleAsync([FromBody] SampleQuery query, IDispatcher dispatcher)
    {
        return TypedResults.Ok(ApiResponse<SampleResponse>.OnSuccess(await dispatcher.Send(query)));
    }
}
