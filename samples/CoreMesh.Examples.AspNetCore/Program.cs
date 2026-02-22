using System.ComponentModel.DataAnnotations;
using CoreMesh.Dispatching;
using CoreMesh.Validation;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDispatching();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapGet("/", ([FromServices] IDispatcher dispatcher) => dispatcher.Send(new SampleQuery("Foo", "Bar")));
app.MapPost("/users", async ([FromServices] IDispatcher dispatcher, CancellationToken ct) =>
{
    var user = new UserCreated(123, "demo@coremesh.dev");

    await dispatcher.Send(user);
    await dispatcher.Publish(user, ct);

    return Results.Ok(new { UserId = user.UserId });
});

app.MapPost("product", async (
    [FromBody] CreateProductCommand command, 
    [FromServices] IDispatcher dispatcher,
    CancellationToken cancellationToken = default) =>
{ 
    await dispatcher.Send(command, cancellationToken);
    
    return Results.Ok();
});

app.Run();

record SampleQuery(string Foo, string Bar) : IRequest<SampleResponse>;
record SampleResponse(string Foo, string Bar);

class SampleHandler : IRequestHandler<SampleQuery, SampleResponse>
{
    public Task<SampleResponse> Handle(SampleQuery request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new SampleResponse(request.Foo, request.Bar));
    }
}

record UserCreated(int UserId, string Email) : INotification, IRequest;

sealed class UserCreatedHandler : IRequestHandler<UserCreated>
{
    public Task Handle(UserCreated request, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Handler!");

        return Task.CompletedTask;
    }
}

sealed class AuditLogOnUserCreatedHandler : INotificationHandler<UserCreated>
{
    public Task Handle(UserCreated notification, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[Audit] User created: {notification.UserId}, {notification.Email}");
        return Task.CompletedTask;
    }
}

sealed class WelcomeEmailOnUserCreatedHandler : INotificationHandler<UserCreated>
{
    public Task Handle(UserCreated notification, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[Mail] Send welcome email to {notification.Email}");
        return Task.CompletedTask;
    }
}

sealed record CreateProductCommand(string Name, decimal Price, string Description): 
    IRequest, 
    IValidatable<CreateProductCommand>
{
    public void ConfigureRules(ValidationBuilder<CreateProductCommand> builder)
    {
        builder.RuleFor(x => x.Name)
            .NotNull()
            .NotEmpty()
            .Length(2, 50);
        
        builder
            .RuleFor(x => x.Description)
            .NotNull()
            .NotEmpty();
    }
}

class CreateProductHandler(
    IValidator<CreateProductCommand> validator
    ): IRequestHandler<CreateProductCommand>
{
    public Task Handle(
        CreateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        validator.ValidateAndThrow(command);
        return Task.CompletedTask;
    }
}
