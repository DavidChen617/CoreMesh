[English](README.md) | 繁體中文

# CoreMesh.Result

`CoreMesh.Result` 為 ASP.NET Core 應用程式提供強型別的 Result Pattern，並透過 Minimal API `TypedResults` 內建 HTTP 回應對應。

它可以幫你：

1. 在 Service 或 Domain 層以 `Result` / `Result<T>` 回傳操作結果
2. 不依賴 ASP.NET Core 的情況下，在業務邏輯層攜帶 HTTP 語意（`Ok`、`NotFound`、`Invalid` 等）
3. 在 API 邊界一行呼叫即可將 Result 轉換為 `IResult`

## 功能

- `Result` / `Result<T>`：非泛型與泛型結果型別
- `Error`：機器可讀的 Code + 人類可讀的 Description
- `ResultStatus`：編碼 HTTP 語意的 enum（`Ok`、`Created`、`NotFound`、`Forbidden`、`Invalid`、`BadRequest`、`NoContent`）
- `ResultExtensions`：靜態工廠方法（`Result.Ok()`、`Result<T>.NotFound(error)` 等）
- `HttpResultExtensions`：將 Result 轉換為 Minimal API 的 `IResult`
- `ApiResponse` / `ApiResponse<T>`：統一的 JSON 回應 envelope
- `ToFile()`：二進制與 Stream 檔案回應
- `ToServerSentEvents()`：SSE 串流回應
- `ToSignIn()` / `ToSignOut()`：ASP.NET Core 驗證回應

## 使用方式

### 建立 Result（Service 層）

```csharp
using CoreMesh.Result.Extensions;

// 成功
return Result<User>.Ok(user);
return Result<User>.Created(user);
return Result.NoContent();

// 失敗
return Result<User>.NotFound(new Error("USER_001", "User not found"));
return Result<User>.Forbidden(new Error("AUTH_001", "Access denied"));
return Result<User>.BadRequest(new Error("INPUT_001", "Invalid input"));

// Validation 錯誤
return Result<User>.Invalid(new Dictionary<string, string[]>
{
    ["Name"]  = ["Name is required", "Name too long"],
    ["Email"] = ["Invalid email format"]
});
```

### 轉換為 HTTP 回應（API 層）

```csharp
using CoreMesh.Result.Http;

app.MapGet("/users/{id}", async (int id, UserService svc) =>
{
    var result = await svc.GetUserAsync(id);
    return result.ToHttpResult();
});
```

### 檔案回應

```csharp
// byte[]
Result<byte[]> result = await fileService.GetPdfAsync(id);
return result.ToFile("application/pdf", "report.pdf");

// Stream
Result<Stream> result = await fileService.GetStreamAsync(id);
return result.ToFile("video/mp4", "video.mp4", enableRangeProcessing: true);
```

### Server-Sent Events

```csharp
// 簡單串流
Result<IAsyncEnumerable<StockPrice>> result = service.GetPriceStream();
return result.ToServerSentEvents(eventType: "price");

// 每筆事件自訂 SseItem
return result.ToServerSentEvents(() =>
    result.Data!.Select(p => new SseItem<StockPrice>(p, eventType: "price")));
```

### 驗證（Auth）

```csharp
Result result = await authService.LoginAsync(req);
return result.ToSignIn(principal, authenticationScheme: "Cookies");

Result result = await authService.LogoutAsync();
return result.ToSignOut();
```

## 回應 Envelope

所有回應都包裝在統一的 `ApiResponse` envelope 中。

### 成功（含資料）

```json
{
  "isSuccess": true,
  "code": "ok",
  "data": { "id": 1, "name": "David" }
}
```

### 成功（無資料）

```json
{
  "isSuccess": true,
  "code": "ok"
}
```

### 失敗

```json
{
  "isSuccess": false,
  "code": "USER_001",
  "problem": {
    "status": 404,
    "title": "USER_001",
    "detail": "User not found"
  }
}
```

### Validation 失敗（422）

```json
{
  "isSuccess": false,
  "code": "VALIDATION",
  "problem": {
    "status": 422,
    "title": "VALIDATION",
    "detail": "One or more validation errors occurred",
    "errors": {
      "Name":  ["Name is required"],
      "Email": ["Invalid email format"]
    }
  }
}
```

## Status → HTTP 對應表

| `ResultStatus` | HTTP 狀態碼              |
|----------------|--------------------------|
| `Ok`           | 200 OK                   |
| `Created`      | 201 Created              |
| `NoContent`    | 204 No Content           |
| `BadRequest`   | 400 Bad Request          |
| `Forbidden`    | 403 Forbidden            |
| `NotFound`     | 404 Not Found            |
| `Invalid`      | 422 Unprocessable Entity |

## 設計說明

- `Result` / `Result<T>` 不依賴 ASP.NET Core，可安全用於 Domain 或 Application 層
- HTTP 相關邏輯（`ApiResponse`、`TypedResults`）集中在 `CoreMesh.Result.Http` namespace
- `NoContent`、`ToFile()`、`ToServerSentEvents()`、`ToSignIn()`、`ToSignOut()` 不包裝 `ApiResponse`，因為它們回傳非 JSON 內容或由 auth middleware 處理
- `Result<T>` 的 constructor 為 `protected`，只能透過 `ResultExtensions` 的工廠方法建立實例
