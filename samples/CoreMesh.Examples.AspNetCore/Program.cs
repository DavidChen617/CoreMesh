using CoreMesh.Dispatching;
using CoreMesh.Validation.Extensions;
using CoreMesh.Examples.AspNetCore.Samples.Products;
using CoreMesh.Examples.AspNetCore.Samples.Queries;
using CoreMesh.Examples.AspNetCore.Samples.Users;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDispatching();
builder.Services.AddValidation();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
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

app.Run();
