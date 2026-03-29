using System.Text.Json;
using CoreMesh.Examples.Outbox.Data;
using CoreMesh.Examples.Outbox.Entities;
using CoreMesh.Examples.Outbox.Outbox;

namespace CoreMesh.Examples.Outbox.Services;

public class TodoService(AppDbContext db)
{
    public async Task CreateAsync(string title, string description, CancellationToken ct = default)
    {
        var todo = new Todo { Title = title, Description = description };

        await db.Todos.AddAsync(todo, ct);
        
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = "todo.created",
            Payload = JsonSerializer.Serialize(new
            {
                TodoId = todo.Id,
                todo.Title
            }),
            OccurredAtUtc = DateTimeOffset.UtcNow,
            Status = OutboxMessageStatus.Pending
        };

        db.OutboxMessages.Add(outboxMessage);

        await db.SaveChangesAsync(ct);
    }
}
