using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Validation;
using CoreMesh.Validation.Abstractions;
using CoreMesh.Validation.Abstractions.Extensions;
using CoreMesh.Validation.Extensions;

namespace CoreMesh.Examples.AspNetCore.Samples.Products;

public sealed record CreateProductCommand(string Name, decimal Price, string Description)
    : IRequest, IValidatable<CreateProductCommand>
{
    public void ConfigureValidateRules(IValidationBuilder<CreateProductCommand> builder)
    {
        builder.For(x => x.Name)
            .NotNull()
            .NotEmpty()
            .MinLength(2)
            .MaxLength(50);

        builder.For(x => x.Description)
            .NotNull()
            .NotEmpty();
    }
}
