[English](README.md) | 繁體中文

# CoreMesh.Logging

`Microsoft.Extensions.Logging` 的檔案日誌 provider，支援自動輪替與 category 過濾。

## 使用方式

```csharp
// 預設選項
builder.Logging.AddFileLogger(builder.Configuration);

// 自訂選項
builder.Logging.AddFileLogger(builder.Configuration, options =>
{
    options.Path = "./Logs/app.log";
    options.MaxFileSize = 1024 * 1024 * 10; // 10 MB
    options.AddFilter(category => category.StartsWith("MyApp"));
});
```

## 選項

| 屬性 | 預設值 | 說明 |
|------|--------|------|
| `Path` | `./Logs/log.txt` | 日誌檔案路徑（目錄不存在時自動建立） |
| `MaxFileSize` | 2048（2 KB） | 觸發輪替的檔案大小上限（bytes） |
| `FileBufferSize` | 2048（2 KB） | 檔案串流緩衝區大小（bytes） |

`AddFilter(predicate)` 限制哪些 category 要寫入檔案，多個 filter 採用 AND 邏輯。

最低日誌等級從設定的 `Logging:LogLevel:Default` 讀取（預設為 `Information`）。

## 日誌格式

```
[2026-01-15 08:30:00] [Information] [MyApp.Services.UserService - User 42 created]
```

## 注意事項

- 日誌條目透過背景佇列（`LogFileProcesser`）非同步寫入。
- 觸發輪替時，現有檔案重新命名並加上時間戳後綴，接著開始新檔案。
- 不支援 `BeginScope`。
