using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Validation;
using CoreMesh.Validation.Abstractions;

namespace CoreMesh.Examples.AspNetCore.Samples.Products;

public sealed class CreateProductHandler(IValidator validator)
    : IRequestHandler<CreateProductCommand>
{
    public Task Handle(CreateProductCommand command, CancellationToken cancellationToken = default)
    {
        var result = validator.Validate(command);
        if (!result.IsValid)
            throw new InvalidOperationException("Validation failed: " + string.Join(", ", result.Errors.SelectMany(e => e.Value)));

        return Task.CompletedTask;
    }
}
