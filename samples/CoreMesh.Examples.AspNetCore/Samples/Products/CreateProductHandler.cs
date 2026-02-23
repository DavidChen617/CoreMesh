using CoreMesh.Dispatching;
using CoreMesh.Validation;

namespace CoreMesh.Examples.AspNetCore.Samples.Products;

public sealed class CreateProductHandler(IValidator<CreateProductCommand> validator)
    : IRequestHandler<CreateProductCommand>
{
    public Task Handle(CreateProductCommand command, CancellationToken cancellationToken = default)
    {
        validator.ValidateAndThrow(command);
        return Task.CompletedTask;
    }
}
