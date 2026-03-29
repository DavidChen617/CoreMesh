using CoreMesh.Outbox.Abstractions;

namespace CoreMesh.Examples.Outbox.Todo;

public class TodoCreatedHandler : IEventHandler<TodoCreatedEvent>
{
    public Task HandleAsync(TodoCreatedEvent @event, CancellationToken cancellationToken)
    {
        Console.WriteLine("Event Created");
        Console.WriteLine(@event.TodoId);
        Console.WriteLine(@event.Title);
        return Task.CompletedTask;
    }
}
