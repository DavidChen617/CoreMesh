using CoreMesh.Dispatching;
using CoreMesh.Mapper;
using CoreMesh.Validation;

namespace CoreMesh.Examples.AspNetCore.Samples.Queries;

public sealed record SampleQuery(string Foo, string Bar) : IRequest<SampleResponse>, IValidatable<SampleQuery>
{
    public void ConfigureRules(ValidationBuilder<SampleQuery> builder)
    {
        builder.RuleFor(query => query.Foo).NotEmpty().NotNull().WithMessage("Foo is required");
        builder.RuleFor(query => query.Bar).NotEmpty().NotNull().WithMessage("Bar is required");
    }
}

public sealed record SampleResponse(string Foo, string Bar): IMapWith<SampleEntity, SampleResponse>
{
    public SampleResponse() : this(string.Empty, string.Empty) { }

    public SampleResponse MapFrom(SampleEntity entity)
    {
        return new SampleResponse(entity.Foo, entity.Bar);
    }

    public SampleEntity MapTo()
    {
        return new (){ Foo = Foo, Bar = Bar };
    }
};

public class SampleEntity
{
    public string Id { get; set; } = string.Empty;
    public string Foo { get; set; } = string.Empty;
    public string Bar { get; set; } = string.Empty;
}

public sealed class SampleHandler(IMapper mapper, IValidator<SampleQuery> validator) : IRequestHandler<SampleQuery, SampleResponse>
{
    public Task<SampleResponse> Handle(
        SampleQuery request,
        CancellationToken cancellationToken = default
        )
    {
        validator.ValidateAndThrow(request);
        
        var sample = new SampleEntity { Id = "123", Foo = request.Foo, Bar = request.Bar };
        
        return Task.FromResult(mapper.Map<SampleEntity, SampleResponse>(sample));
    }
}
