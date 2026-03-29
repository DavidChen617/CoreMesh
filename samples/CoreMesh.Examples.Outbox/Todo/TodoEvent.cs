using CoreMesh.Outbox.Abstractions;

namespace CoreMesh.Examples.Outbox.Todo;

[EventName("todo.created")]
public sealed record TodoCreatedEvent : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
    public Guid TodoId { get; set; }
    public string Title { get; set; } = string.Empty;
}
