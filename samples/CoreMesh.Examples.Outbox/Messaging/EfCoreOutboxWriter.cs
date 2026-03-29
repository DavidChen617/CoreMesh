using CoreMesh.Examples.Outbox.Data;
using CoreMesh.Outbox.Abstractions;

namespace CoreMesh.Examples.Outbox.Messaging;

public class EfCoreOutboxWriter(AppDbContext db) : IOutboxWriter
{
    public async Task AddAsync(IEvent @event, CancellationToken cancellationToken = default)
    {
        var message = OutboxMessage.Create(@event);
        await db.OutboxMessages.AddAsync(message, cancellationToken);
    }
}
