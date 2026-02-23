using CoreMesh.Dispatching;

namespace CoreMesh.Examples.AspNetCore.Samples.Queries;

public sealed record SampleQuery(string Foo, string Bar) : IRequest<SampleResponse>;

public sealed record SampleResponse(string Foo, string Bar);

public sealed class SampleHandler : IRequestHandler<SampleQuery, SampleResponse>
{
    public Task<SampleResponse> Handle(SampleQuery request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new SampleResponse(request.Foo, request.Bar));
    }
}
