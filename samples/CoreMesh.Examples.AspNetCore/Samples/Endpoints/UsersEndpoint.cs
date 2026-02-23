using CoreMesh.Dispatching;
using CoreMesh.Endpoints;
using CoreMesh.Examples.AspNetCore.Samples.Users;
using CoreMesh.Http.Responses;

namespace CoreMesh.Examples.AspNetCore.Samples.Endpoints;

public sealed class UsersEndpoint : IEndpoint
{
    public void AddRoute(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder app)
    {
        app.MapPost("/users", async (IDispatcher dispatcher, CancellationToken ct) =>
        {
            var user = new UserCreated(123, "demo@coremesh.dev");

            await dispatcher.Send(user);
            await dispatcher.Publish(user, ct);

            return TypedResults.Ok(ApiResponse<object>.OnSuccess(new { user.UserId }));
        });
    }
}
