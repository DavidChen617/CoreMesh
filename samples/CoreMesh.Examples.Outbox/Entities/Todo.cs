namespace CoreMesh.Examples.Outbox.Entities;

public sealed class Todo
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } =  string.Empty;
    public bool IsCompleted { get; set; } = false;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
