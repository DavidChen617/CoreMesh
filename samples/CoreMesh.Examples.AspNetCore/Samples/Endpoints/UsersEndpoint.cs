using CoreMesh.Dispatching;
using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Endpoints;
using CoreMesh.Examples.AspNetCore.Samples.Users;
using CoreMesh.Http.Responses;

namespace CoreMesh.Examples.AspNetCore.Samples.Endpoints;

public sealed class UsersEndpoint : IGroupedEndpoint<UserGroupEndpoint>
{
    public void AddRoute(RouteGroupBuilder group)
    {
        group.MapPost("", HandleAsync)
            .MapToApiVersion(2);
    }

    private static async Task<IResult> HandleAsync(IDispatcher dispatcher, CancellationToken ct)
    {
        var user = new UserCreated(123, "demo@coremesh.dev");

        await dispatcher.Send(user, ct);
        await dispatcher.Publish(user, ct);

        return TypedResults.Ok(ApiResponse<object>.OnSuccess(new { user.UserId }));
    }
}
