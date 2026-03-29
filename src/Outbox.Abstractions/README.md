# CoreMesh.Outbox.Abstractions

Core interfaces and models for the CoreMesh Outbox pattern.
Reference this package to define events and handlers without taking a dependency on any infrastructure.

## Installation

```bash
dotnet add package CoreMesh.Outbox.Abstractions
```

---

## Interfaces

### Event Definition

| Type | Role |
|---|---|
| `IEvent` | Marker interface for domain events. Requires `Id` and `OccurredAtUtc`. |
| `EventNameAttribute` | Decorates an event class with its logical type name used for routing and serialization. |

```csharp
[EventName("order.created")]
public sealed record OrderCreatedEvent : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
    public Guid OrderId { get; init; }
}
```

---

### Event Handling

| Type | Role |
|---|---|
| `IEventHandler<TEvent>` | Implement to handle a specific event type. |
| `IEventHandler` | Non-generic base used internally for type-erased dispatch. |
| `IEventDispatcher` | Resolves and invokes the correct handler for an incoming `EventEnvelope`. |
| `IEventTypeRegistry` | Maps event type name strings to CLR types. Populated at startup by scanning assemblies. |

```csharp
public class OrderCreatedHandler : IEventHandler<OrderCreatedEvent>
{
    public Task HandleAsync(OrderCreatedEvent @event, CancellationToken ct)
    {
        // handle the event
        return Task.CompletedTask;
    }
}
```

---

### Outbox Write Side

| Type | Role |
|---|---|
| `IOutboxWriter` | Write an event into the outbox store as part of the current unit of work. |
| `IOutboxStore` | Infrastructure interface for claiming, updating, and recovering outbox messages. |
| `OutboxMessage` | The persisted outbox record. Created via `OutboxMessage.Create(event)`. |
| `OutboxMessageStatus` | `Pending` → `Processing` → `Processed` / `Failed` |

#### IOutboxWriter — Unit of Work Contract

`AddAsync` does **not** persist immediately. It participates in the caller's existing transaction.
The caller is responsible for committing (e.g. `DbContext.SaveChangesAsync`) to guarantee atomicity.

```csharp
await db.Orders.AddAsync(order, ct);
await writer.AddAsync(new OrderCreatedEvent { OrderId = order.Id }, ct);
await db.SaveChangesAsync(ct);  // both saved atomically
```

---

### Message Transport

| Type | Role |
|---|---|
| `IEventPublisher` | Publish an `OutboxMessage` to the message broker. Implement per broker. |
| `IMessageSubscriber` | Subscribe to incoming messages and control Ack / Nack / Retry lifecycle. |
| `EventEnvelope` | Wraps a received message with its type, payload, timestamp, and transport headers. |

#### IMessageSubscriber — Ack / Nack / Retry Contract

```
Dispatch succeeded
  └─ AckAsync(envelope)   → commit broker offset, message will not be re-delivered

Dispatch failed, retries remaining
  └─ RetryAsync(envelope) → seek back broker offset, message will be re-delivered

Dispatch failed, max retries exceeded
  └─ NackAsync(envelope)  → forward to dead letter destination, commit offset
```
