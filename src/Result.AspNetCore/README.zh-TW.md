[English](README.md) | 繁體中文

# CoreMesh.Result.AspNetCore

`CoreMesh.Result` 的 ASP.NET Core 整合層。提供全域 exception 處理、HTTP 回應轉換，以及一致的 `ApiResponse` envelope。

## 註冊

```csharp
builder.Services.AddCoreMeshExceptionHandling();
app.UseCoreMeshExceptionHandling();
```

依序註冊 `ProblemDetails` 與以下 exception handler：

| Handler | Exception | HTTP Status |
|---------|-----------|-------------|
| `ValidationExceptionHandler` | `ValidationException` | 422 |
| `ConflictExceptionHandler` | `ConflictException` | 409 |
| `ForbiddenExceptionHandler` | `ForbiddenException` | 403 |
| `NotFoundExceptionHandler` | `NotFoundException` | 404 |
| `GlobalExceptionHandler` | 其他未處理的 exception | 400 / 401 / 500 |

## 將 Result 轉換為 HTTP 回應

```csharp
// 非泛型 Result → 200/204/400/404/...
result.ToHttpResult()

// Result<T> → ResultHttpResult<T>（含 OpenAPI metadata，不需手動 .Produces<T>()）
result.ToHttpResult()

// 其他輔助方法
result.ToJson(statusCode: 201)
result.ToFile("application/pdf", "report.pdf")         // Result<byte[]>
result.ToFile("video/mp4", "video.mp4")                // Result<Stream>
result.ToServerSentEvents(eventType: "price")           // Result<IAsyncEnumerable<T>>
result.ToSignIn(principal, authenticationScheme: "...")
result.ToSignOut()
```

## Status → HTTP 對應

| `ResultStatus` | HTTP |
|----------------|------|
| `Ok` | 200 |
| `Created` | 201 |
| `NoContent` | 204 |
| `BadRequest` | 400 |
| `Forbidden` | 403 |
| `NotFound` | 404 |
| `Invalid` | 422 |
| 其他 | 500 |

## ApiResponse Envelope

```json
// 成功
{ "isSuccess": true, "code": "ok", "data": { ... } }

// 失敗
{ "isSuccess": false, "code": "USER_001", "problem": { "status": 404, "detail": "..." } }

// 驗證失敗
{ "isSuccess": false, "code": "VALIDATION", "problem": { "status": 422, "errors": { ... } } }
```

`NoContent`、`ToFile()`、`ToServerSentEvents()`、`ToSignIn()`、`ToSignOut()` 不使用 envelope。

## GlobalExceptionHandler

- **Development**：回傳原始 exception message
- **Production**：非 `AppException` 回傳通用訊息
- 永遠以 `Error` 等級記錄並附上 `TraceId`
