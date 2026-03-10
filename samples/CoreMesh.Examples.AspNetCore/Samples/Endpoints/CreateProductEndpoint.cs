using CoreMesh.Dispatching;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using CoreMesh.Examples.AspNetCore.Samples.Products;
using CoreMesh.Http.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CoreMesh.Examples.AspNetCore.Samples.Endpoints;

public sealed class CreateProductEndpoint : IEndpoint
{
    public void AddRoute(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder app)
    {
        app.MapPost("product", async (
            [FromBody] CreateProductCommand command,
            IDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            await dispatcher.Send(command, cancellationToken);
            return TypedResults.Ok(ApiResponse.OnSuccess());
        });
    }
}
