using Asp.Versioning;
using CoreMesh.Endpoints;

namespace CoreMesh.Examples.AspNetCore.Samples.Endpoints;

public sealed class UserGroupEndpoint : IGroupEndpoint
{
    public string GroupPrefix { get; } = "api/v{version:apiVersion}/users";

    public void Configure(RouteGroupBuilder group)
    {
        var apiVersionSet = group.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .HasApiVersion(new ApiVersion(2))
            .ReportApiVersions()
            .Build();

        group.WithApiVersionSet(apiVersionSet);
    }
}
