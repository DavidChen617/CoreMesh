# CoreMesh.Outbox

基於 .NET Generic Host 的 Transactional Outbox Pattern 實作。
透過在同一個交易內將事件與領域變更一起持久化，確保訊息至少被投遞一次（at-least-once delivery）。

## 安裝

```bash
dotnet add package CoreMesh.Outbox
```

## 快速開始

```csharp
services.AddCoreMeshOutbox(
    [typeof(Program).Assembly],
    options =>
    {
        options.UseInMemoryStore();        // 或 AddOutboxStore<T> / AddOutboxWriter<T>
        options.UseInMemoryChannel();      // 或 AddMessageQueue<TPublisher, TConsumer>
        options.WithConsumer();            // 可選：此應用程式需要消費訊息時才加
    });
```

定義事件：

```csharp
[EventName("order.created")]
public sealed record OrderCreatedEvent : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
    public Guid OrderId { get; init; }
}
```

寫入 Outbox（與實體儲存在同一個交易）：

```csharp
public class OrderService(AppDbContext db, IOutboxWriter writer)
{
    public async Task CreateAsync(CreateOrderCommand cmd, CancellationToken ct = default)
    {
        var order = new Order { ... };
        await db.Orders.AddAsync(order, ct);
        await writer.AddAsync(new OrderCreatedEvent { OrderId = order.Id }, ct);
        await db.SaveChangesAsync(ct);   // 訂單與 outbox 訊息在同一個交易內儲存
    }
}
```

實作事件處理器：

```csharp
public class OrderCreatedHandler : IEventHandler<OrderCreatedEvent>
{
    public Task HandleAsync(OrderCreatedEvent @event, CancellationToken ct)
    {
        // 處理事件
        return Task.CompletedTask;
    }
}
```

---

## 流程說明

### 寫入與發布（Producer 端）

```
Application
  │
  ├─ 1. IOutboxWriter.AddAsync(event)
  │       └─ 將 OutboxMessage (Status=Pending) 寫入 Store
  │          ↳ 與領域實體同一個交易 — 原子性保證
  │
  └─ 2. SaveChanges()  ← 同時 commit 領域實體與 outbox 訊息

OutboxDispatcher（BackgroundService，每 5 秒輪詢一次）
  │
  ├─ 3. IOutboxStore.ClaimBatchAsync(100)
  │       └─ Pending → Processing（原子操作，多實例部署安全）
  │
  └─ 4. 逐一處理已搶佔的訊息：
          ├─ IEventPublisher.PublishAsync(message)  → Message Broker
          ├─ 成功 → IOutboxStore.MarkProcessedAsync(id)
          └─ 失敗 → IOutboxStore.MarkFailedAsync(id, nextRetryAt)
```

---

### 消費與派發（Consumer 端）

```
Message Broker
  │
  └─ 1. IMessageSubscriber.SubscribeAsync()  → 產生 EventEnvelope 串流

EventConsumer（BackgroundService）
  │
  └─ 2. IEventDispatcher.DispatchAsync(envelope)
          └─ 反序列化 payload → IEventHandler<TEvent>.HandleAsync(event)
                ├─ 成功              → IMessageSubscriber.AckAsync()    → commit offset
                ├─ 失敗（未達上限）   → IMessageSubscriber.RetryAsync()  → seek back offset，下次重新消費
                └─ 失敗（達到上限）   → IMessageSubscriber.NackAsync()   → 送至 Dead Letter，commit offset
```

---

### 重試與 Dead Letter

```
第 1 次 → DispatchAsync → 例外
           └─ RetryAsync：seek back offset，下次 Consume 重新投遞

第 2 次 → DispatchAsync → 例外
           └─ RetryAsync：seek back offset，下次 Consume 重新投遞

第 3 次 → DispatchAsync → 例外
           └─ NackAsync：轉送至 Dead Letter Topic，commit offset 繼續往前
```

預設 `MaxRetries = 3`，可繼承 `EventConsumer` 覆寫。

---

### Zombie 訊息復原

```
OutboxDispatcher（每 5 分鐘執行一次）
  │
  └─ IOutboxStore.ResetZombiesAsync(timeout = 5min)
          └─ 卡在 Processing 超過 timeout 的訊息
             → 重置回 Pending，等待下次被搶佔

Zombie 訊息的成因：Dispatcher 實例在 PublishAsync 之後、
MarkProcessedAsync 之前崩潰。
透過 ClaimId + ProcessingStartedAt 偵測並復原這些訊息。
```

---

## 自訂 Infrastructure

實作 `IOutboxStore`、`IOutboxWriter` 對接你的資料庫，以及 `IEventPublisher`、`IMessageSubscriber` 對接你的 Message Broker：

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
