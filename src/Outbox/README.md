# CoreMesh.Outbox

Transactional Outbox pattern implementation for .NET using the Generic Host.
Guarantees at-least-once event delivery by persisting events atomically with domain changes before publishing to a message broker.

## Installation

```bash
dotnet add package CoreMesh.Outbox
```

## Getting Started

```csharp
services.AddCoreMeshOutbox(
    [typeof(Program).Assembly],
    options =>
    {
        options.UseInMemoryStore();        // or AddOutboxStore<T> / AddOutboxWriter<T>
        options.UseInMemoryChannel();      // or AddMessageQueue<TPublisher, TConsumer>
        options.WithConsumer();            // optional: only if this app consumes messages
    });
```

Define an event:

```csharp
[EventName("order.created")]
public sealed record OrderCreatedEvent : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
    public Guid OrderId { get; init; }
}
```

Write to outbox within the same transaction as your entity save:

```csharp
public class OrderService(AppDbContext db, IOutboxWriter writer)
{
    public async Task CreateAsync(CreateOrderCommand cmd, CancellationToken ct = default)
    {
        var order = new Order { ... };
        await db.Orders.AddAsync(order, ct);
        await writer.AddAsync(new OrderCreatedEvent { OrderId = order.Id }, ct);
        await db.SaveChangesAsync(ct);   // order + outbox message saved atomically
    }
}
```

Handle the event on the consumer side:

```csharp
public class OrderCreatedHandler : IEventHandler<OrderCreatedEvent>
{
    public Task HandleAsync(OrderCreatedEvent @event, CancellationToken ct)
    {
        // ...
        return Task.CompletedTask;
    }
}
```

---

## Flow

### Write & Publish (Producer side)

```
Application
  │
  ├─ 1. IOutboxWriter.AddAsync(event)
  │       └─ Insert OutboxMessage (Status=Pending) into store
  │          ↳ same transaction as domain entity — atomic
  │
  └─ 2. SaveChanges()  ← commits both domain entity and outbox message

OutboxDispatcher  (BackgroundService, polls every 5s)
  │
  ├─ 3. IOutboxStore.ClaimBatchAsync(100)
  │       └─ Pending → Processing  (atomic, safe for multi-instance)
  │
  └─ 4. For each claimed message:
          ├─ IEventPublisher.PublishAsync(message)  → Message Broker
          ├─ on success → IOutboxStore.MarkProcessedAsync(id)
          └─ on failure → IOutboxStore.MarkFailedAsync(id, nextRetryAt)
```

---

### Consume & Dispatch (Consumer side)

```
Message Broker
  │
  └─ 1. IMessageSubscriber.SubscribeAsync()  → yields EventEnvelope

EventConsumer  (BackgroundService)
  │
  └─ 2. IEventDispatcher.DispatchAsync(envelope)
          └─ Deserialize payload → IEventHandler<TEvent>.HandleAsync(event)
                ├─ on success → IMessageSubscriber.AckAsync()   → commit offset
                ├─ on failure (attempt < max) → IMessageSubscriber.RetryAsync() → seek back offset
                └─ on failure (attempt = max) → IMessageSubscriber.NackAsync()  → dead letter + commit offset
```

---

### Retry & Dead Letter

```
Attempt 1  →  DispatchAsync  →  Exception
               └─ RetryAsync: seek back offset, re-deliver on next consume

Attempt 2  →  DispatchAsync  →  Exception
               └─ RetryAsync: seek back offset, re-deliver on next consume

Attempt 3  →  DispatchAsync  →  Exception
               └─ NackAsync:  forward to dead letter topic, commit offset
```

Default `MaxRetries = 3`. Override by subclassing `EventConsumer`.

---

### Zombie Recovery

```
OutboxDispatcher  (every 5 minutes)
  │
  └─ IOutboxStore.ResetZombiesAsync(timeout = 5min)
          └─ Messages stuck in Processing longer than timeout
             → reset to Pending for re-pickup

A zombie occurs when a dispatcher instance crashes between
PublishAsync and MarkProcessedAsync. ClaimId + ProcessingStartedAt
are used to detect and recover these messages.
```

---

## Custom Infrastructure

Implement `IOutboxStore` + `IOutboxWriter` for your database and `IEventPublisher` + `IMessageSubscriber` for your broker:

```csharp
services.AddCoreMeshOutbox(
    [typeof(Program).Assembly],
    options =>
    {
        options.AddOutboxStore<MyEfCoreOutboxStore>()
               .AddOutboxWriter<MyEfCoreOutboxWriter>();
        options.AddMessageQueue<MyKafkaPublisher, MyKafkaSubscriber>();
        options.WithConsumer();
    });
```
