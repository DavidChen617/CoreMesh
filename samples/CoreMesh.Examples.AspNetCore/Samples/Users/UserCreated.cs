using CoreMesh.Dispatching.Abstractions;

namespace CoreMesh.Examples.AspNetCore.Samples.Users;

public sealed record UserCreated(int UserId, string Email) : INotification, IRequest;
