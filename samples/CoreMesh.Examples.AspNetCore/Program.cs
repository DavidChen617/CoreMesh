using CoreMesh.Dispatching;
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
