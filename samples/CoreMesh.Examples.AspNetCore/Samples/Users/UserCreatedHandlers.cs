using CoreMesh.Dispatching;

namespace CoreMesh.Examples.AspNetCore.Samples.Users;

public sealed class UserCreatedHandler : IRequestHandler<UserCreated>
{
    public Task Handle(UserCreated request, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Handler!");
        return Task.CompletedTask;
    }
}

public sealed class AuditLogOnUserCreatedHandler : INotificationHandler<UserCreated>
{
    public Task Handle(UserCreated notification, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[Audit] User created: {notification.UserId}, {notification.Email}");
        return Task.CompletedTask;
    }
}

public sealed class WelcomeEmailOnUserCreatedHandler : INotificationHandler<UserCreated>
{
    public Task Handle(UserCreated notification, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[Mail] Send welcome email to {notification.Email}");
        return Task.CompletedTask;
    }
}
