using CoreMesh.Endpoints;
using CoreMesh.Http.Responses;

namespace CoreMesh.Examples.AspNetCore.Samples.Endpoints;

public sealed class ProductsEndpoint : IEndpoint
{
    public void AddRoute(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder app)
    {
        app.MapGet("/products/{id:int}", (int id) =>
        {
            var data = new ProductDto { Id = id, Name = "Book" };
            return TypedResults.Ok(ApiResponse<ProductDto>.OnSuccess(data, code: "ok"));
        });
    }

    private sealed class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
