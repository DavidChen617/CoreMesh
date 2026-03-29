# CoreMesh.Outbox.Abstractions

CoreMesh Outbox Pattern 的核心介面與模型。
引用此套件即可定義事件與處理器，不需依賴任何 infrastructure 實作。

## 安裝

```bash
dotnet add package CoreMesh.Outbox.Abstractions
```

---

## 介面說明

### 事件定義

| 類型 | 職責 |
|---|---|
| `IEvent` | 領域事件的標記介面，需提供 `Id` 與 `OccurredAtUtc`。 |
| `EventNameAttribute` | 標記事件類別的邏輯型別名稱，用於路由與序列化。 |

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

### 事件處理

| 類型 | 職責 |
|---|---|
| `IEventHandler<TEvent>` | 實作此介面以處理特定事件類型。 |
| `IEventHandler` | 非泛型基底介面，供內部型別擦除派發使用。 |
| `IEventDispatcher` | 解析並呼叫對應的 Handler 處理傳入的 `EventEnvelope`。 |
| `IEventTypeRegistry` | 將事件型別名稱字串對應至 CLR 型別，於啟動時掃描 Assembly 建立。 |

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

### Outbox 寫入端

| 類型 | 職責 |
|---|---|
| `IOutboxWriter` | 將事件寫入 outbox，作為當前 Unit of Work 的一部分。 |
| `IOutboxStore` | 提供搶佔、更新、復原 outbox 訊息的 infrastructure 介面。 |
| `OutboxMessage` | 持久化的 outbox 紀錄，透過 `OutboxMessage.Create(event)` 建立。 |
| `OutboxMessageStatus` | `Pending` → `Processing` → `Processed` / `Failed` |

#### IOutboxWriter — Unit of Work 契約

`AddAsync` **不會立即持久化**，它參與呼叫端現有的交易。
呼叫端需負責 commit（例如 `DbContext.SaveChangesAsync`）以確保原子性。

```csharp
await db.Orders.AddAsync(order, ct);
await writer.AddAsync(new OrderCreatedEvent { OrderId = order.Id }, ct);
await db.SaveChangesAsync(ct);  // 兩者在同一個交易內儲存
```

---

### 訊息傳輸

| 類型 | 職責 |
|---|---|
| `IEventPublisher` | 將 `OutboxMessage` 發布至 Message Broker，依 Broker 實作。 |
| `IMessageSubscriber` | 訂閱傳入訊息，並控制 Ack / Nack / Retry 的生命週期。 |
| `EventEnvelope` | 封裝接收到的訊息，包含型別、payload、時間戳記與 transport headers。 |

#### IMessageSubscriber — Ack / Nack / Retry 契約

```
派發成功
  └─ AckAsync(envelope)   → commit broker offset，訊息不再重送

派發失敗，尚有重試次數
  └─ RetryAsync(envelope) → seek back broker offset，下次 Consume 重新投遞

派發失敗，已達重試上限
  └─ NackAsync(envelope)  → 轉送至 Dead Letter，commit offset 繼續往前
```
