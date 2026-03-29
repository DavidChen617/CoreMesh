using CoreMesh.Examples.Outbox.Data;
using CoreMesh.Outbox.Abstractions;

namespace CoreMesh.Examples.Outbox.Todo;

public record CreateTodoCommand(string Title, string Description);

public class TodoService(
    AppDbContext db,
    IOutboxWriter writer)
{
    public async Task CreateAsync(CreateTodoCommand command, CancellationToken ct = default)
    {
        var todo = new Entities.Todo { Title = command.Title, Description = command.Description };

        await db.Todos.AddAsync(todo, ct);

        await writer.AddAsync(new TodoCreatedEvent { TodoId = todo.Id, Title = command.Title }, ct);

        await db.SaveChangesAsync(ct);
    }
}
