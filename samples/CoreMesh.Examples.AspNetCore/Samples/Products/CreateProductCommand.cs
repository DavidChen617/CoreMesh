using CoreMesh.Dispatching;
using CoreMesh.Validation;

namespace CoreMesh.Examples.AspNetCore.Samples.Products;

public sealed record CreateProductCommand(string Name, decimal Price, string Description)
    : IRequest, IValidatable<CreateProductCommand>
{
    public void ConfigureRules(ValidationBuilder<CreateProductCommand> builder)
    {
        builder.RuleFor(x => x.Name)
            .NotNull()
            .NotEmpty()
            .Length(2, 50);

        builder.RuleFor(x => x.Description)
            .NotNull()
            .NotEmpty();
    }
}
