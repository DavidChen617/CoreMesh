using CoreMesh.Dispatching;
using CoreMesh.Validation.Extensions;
using CoreMesh.Examples.AspNetCore.Samples.Products;
using CoreMesh.Examples.AspNetCore.Samples.Queries;
using CoreMesh.Examples.AspNetCore.Samples.Users;
using CoreMesh.Http.Extensions;
using CoreMesh.Http.Responses;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services
    .AddDispatching()
    .AddValidation()
    .AddCoreMeshHttp();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCoreMeshHttp();

app.MapGet("/", ([FromServices] IDispatcher dispatcher) => dispatcher.Send(new SampleQuery("Foo", "Bar")));
app.MapPost("/users", async ([FromServices] IDispatcher dispatcher, CancellationToken ct) =>
{
    var user = new UserCreated(123, "demo@coremesh.dev");

    await dispatcher.Send(user);
    await dispatcher.Publish(user, ct);

    return Results.Ok(new { UserId = user.UserId });
});

app.MapPost("product", async (
    [FromBody] CreateProductCommand command, 
    [FromServices] IDispatcher dispatcher,
    CancellationToken cancellationToken = default) =>
{ 
    await dispatcher.Send(command, cancellationToken);
    
    return Results.Ok();
});

app.MapGet("/products/{id:int}", (int id) =>
{
    var data = new Product { Id = id, Name = "Book" };
    // throw new NotFoundException("Product", id);
    return TypedResults.Ok(ApiResponse<Product>.OnSuccess(data, code: "ok"));
});

app.Run();

class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
}
