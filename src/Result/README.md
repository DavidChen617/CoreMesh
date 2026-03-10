[繁體中文](README.zh-TW.md) | English

# CoreMesh.Result

`CoreMesh.Result` provides a strongly-typed result pattern for ASP.NET Core applications, with built-in HTTP response mapping via Minimal API `TypedResults`.

It allows you to:

1. Return operation outcomes as `Result` / `Result<T>` from service or domain layers
2. Carry HTTP semantics (`Ok`, `NotFound`, `Invalid`, etc.) without depending on ASP.NET Core in your business logic
3. Convert results to `IResult` at the API boundary with a single call

## Features

- `Result` / `Result<T>`: non-generic and generic result types
- `Error`: machine-readable code + human-readable description
- `ResultStatus`: enum encoding the HTTP outcome (`Ok`, `Created`, `NotFound`, `Forbidden`, `Invalid`, `BadRequest`, `NoContent`)
- `ResultExtensions`: static factory methods (`Result.Ok()`, `Result<T>.NotFound(error)`, etc.)
- `HttpResultExtensions`: converts results to `IResult` for Minimal API endpoints
- `ApiResponse` / `ApiResponse<T>`: unified JSON response envelope
- `ToFile()`: binary and stream file responses
- `ToServerSentEvents()`: SSE streaming responses
- `ToSignIn()` / `ToSignOut()`: ASP.NET Core authentication responses

## Usage

### Creating Results (service layer)

```csharp
using CoreMesh.Result.Extensions;

// Success
return Result<User>.Ok(user);
return Result<User>.Created(user);
return Result.NoContent();

// Failure
return Result<User>.NotFound(new Error("USER_001", "User not found"));
return Result<User>.Forbidden(new Error("AUTH_001", "Access denied"));
return Result<User>.BadRequest(new Error("INPUT_001", "Invalid input"));

// Validation errors
return Result<User>.Invalid(new Dictionary<string, string[]>
{
    ["Name"]  = ["Name is required", "Name too long"],
    ["Email"] = ["Invalid email format"]
});
```

### Converting to HTTP Response (API layer)

```csharp
using CoreMesh.Result.Http;

app.MapGet("/users/{id}", async (int id, UserService svc) =>
{
    var result = await svc.GetUserAsync(id);
    return result.ToHttpResult();
});
```

### File Response

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
// Simple stream
Result<IAsyncEnumerable<StockPrice>> result = service.GetPriceStream();
return result.ToServerSentEvents(eventType: "price");

// Custom SseItem per event
return result.ToServerSentEvents(() =>
    result.Data!.Select(p => new SseItem<StockPrice>(p, eventType: "price")));
```

### Authentication

```csharp
Result result = await authService.LoginAsync(req);
return result.ToSignIn(principal, authenticationScheme: "Cookies");

Result result = await authService.LogoutAsync();
return result.ToSignOut();
```

## Response Envelope

All responses are wrapped in a consistent `ApiResponse` envelope.

### Success (with data)

```json
{
  "isSuccess": true,
  "code": "ok",
  "data": { "id": 1, "name": "David" }
}
```

### Success (no data)

```json
{
  "isSuccess": true,
  "code": "ok"
}
```

### Failure

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

### Validation Failure (422)

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

## Status → HTTP Mapping

| `ResultStatus` | HTTP Status              |
|----------------|--------------------------|
| `Ok`           | 200 OK                   |
| `Created`      | 201 Created              |
| `NoContent`    | 204 No Content           |
| `BadRequest`   | 400 Bad Request          |
| `Forbidden`    | 403 Forbidden            |
| `NotFound`     | 404 Not Found            |
| `Invalid`      | 422 Unprocessable Entity |

## Design Notes

- The `Result` / `Result<T>` types have no dependency on ASP.NET Core — they are safe to use in domain or application layers
- HTTP concerns (`ApiResponse`, `TypedResults`) are isolated in the `CoreMesh.Result.Http` namespace
- `NoContent`, `ToFile()`, `ToServerSentEvents()`, `ToSignIn()`, and `ToSignOut()` do not wrap in `ApiResponse` since they carry non-JSON payloads or are handled by the auth middleware
- `Result<T>` constructors are `protected` — instances can only be created through factory methods in `ResultExtensions`
