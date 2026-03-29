namespace CoreMesh.Outbox.Abstractions;

/// <summary>
/// Writes domain events into the outbox store as part of the current unit of work.
/// </summary>
/// <remarks>
/// <see cref="AddAsync"/> does not persist immediately — it participates in the caller's
/// existing transaction. The caller is responsible for committing the unit of work
/// (e.g. calling <c>DbContext.SaveChangesAsync</c>) to ensure atomicity between the
/// domain entity change and the outbox entry.
/// </remarks>
public interface IOutboxWriter
{
    /// <summary>
    /// Adds the given <paramref name="event"/> to the outbox as a pending message.
    /// </summary>
    Task AddAsync(IEvent @event, CancellationToken cancellationToken = default);
}
