using Asp.Versioning;
using CoreMesh.Dispatching.Extensions;
using CoreMesh.Endpoints.Extensions;
using CoreMesh.Mapper.Extensions;
using CoreMesh.Validation.Extensions;
using CoreMesh.Http.Extensions;
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpoints()
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1);
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    }).AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'V";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddOpenApi();

builder.Services
    .AddDispatching([typeof(Program).Assembly])
    .AddCoreMeshMapper([typeof(Program).Assembly])
    .AddValidation()
    .AddCoreMeshHttp();

// builder.Services.AddLogging(logging => { });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCoreMeshHttp();
app.MapEndpoints();

app.Run();
