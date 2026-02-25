using CoreMesh.Mapper;

namespace CoreMesh.Examples.AspNetCore.Samples.Mapper;

public sealed class MapperUser
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class MapperUserProfile
{
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public sealed class MapperUserStats
{
    public int OrderCount { get; set; }
}

public sealed record MapperUserDto(string FullName, string Email)
    : IMapFrom<MapperUser, MapperUserDto>
{
    public MapperUserDto() : this(string.Empty, string.Empty)
    {
    }

    public MapperUserDto MapFrom(MapperUser source)
        => new($"{source.FirstName} {source.LastName}", source.Email);
}

public sealed record MapperUserAggregateDto(string FullName, string Email, string City, string Country)
    : IMapFrom<MapperUser, MapperUserProfile, MapperUserAggregateDto>
{
    public MapperUserAggregateDto() : this(string.Empty, string.Empty, string.Empty, string.Empty)
    {
    }

    public MapperUserAggregateDto MapFrom(MapperUser user, MapperUserProfile profile)
        => new($"{user.FirstName} {user.LastName}", user.Email, profile.City, profile.Country);
}

public sealed record MapperUserSummaryDto(string FullName, string Email, string City, int OrderCount)
    : IMapFrom<MapperUser, MapperUserProfile, MapperUserStats, MapperUserSummaryDto>
{
    public MapperUserSummaryDto() : this(string.Empty, string.Empty, string.Empty, 0)
    {
    }

    public MapperUserSummaryDto MapFrom(MapperUser user, MapperUserProfile profile, MapperUserStats stats)
        => new($"{user.FirstName} {user.LastName}", user.Email, profile.City, stats.OrderCount);
}
