using CoreMesh.Dispatching;
using CoreMesh.Endpoints.Extensions;
using CoreMesh.Validation.Extensions;
using CoreMesh.Http.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services
    .AddDispatching()
    .AddEndpoints()
    .AddValidation()
    .AddCoreMeshHttp();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCoreMeshHttp();
app.MapEndpoints();

app.Run();
