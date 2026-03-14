using Asp.Versioning;
using CoreMesh.Dispatching.Extensions;
using CoreMesh.Endpoints.Extensions;
using CoreMesh.Mapper.Extensions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using CoreMesh.Result.Http;
using CoreMesh.Validation.Extensions;
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpoints([typeof(Program).Assembly])
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
    .AddValidatable()
    .AddCoreMeshExceptionHandling();

// builder.Services.AddLogging(logging => { });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost("test", (Test1 t) => Result<Test1>.Ok(new Test1 { Test = t.Test }).ToHttpResult());

app.UseHttpsRedirection();
app.UseCoreMeshExceptionHandling();
app.MapEndpoints();

app.Run();


class Test1
{
    public sbyte Test { get; set; }
}
class Test2
{
    public sbyte Test { get; set; }
}
