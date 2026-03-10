using CoreMesh.Endpoints;
using CoreMesh.Examples.AspNetCore.Samples.Mapper;
using CoreMesh.Result.Http;
using CoreMesh.Mapper;

namespace CoreMesh.Examples.AspNetCore.Samples.Endpoints;

public sealed class MapperSampleEndpoint : IEndpoint
{
    public void AddRoute(IEndpointRouteBuilder app)
    {
        app.MapGet("/mapper/demo", HandleAsync);
    }

    private static IResult HandleAsync(IMapper mapper)
    {
        var user = new MapperUser
        {
            Id = "1",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Password = "secret"
        };

        var profile = new MapperUserProfile
        {
            City = "Taipei",
            Country = "Taiwan"
        };

        var stats = new MapperUserStats
        {
            OrderCount = 42
        };

        var users = Enumerable.Range(1, 3)
            .Select(i => new MapperUser
            {
                Id = i.ToString(),
                FirstName = $"User{i}",
                LastName = "Sample",
                Email = $"user{i}@example.com",
                Password = $"pwd{i}"
            })
            .ToList();

        var single = mapper.Map<MapperUser, MapperUserDto>(user);
        var aggregate = mapper.Map<MapperUser, MapperUserProfile, MapperUserAggregateDto>(user, profile);
        var summary = mapper.Map<MapperUser, MapperUserProfile, MapperUserStats, MapperUserSummaryDto>(user, profile, stats);
        var list = mapper.Map<MapperUser, MapperUserDto>(users).ToList();

        return TypedResults.Ok(ApiResponse<object>.OnSuccess(new
        {
            single,
            aggregate,
            summary,
            list
        }));
    }
}
