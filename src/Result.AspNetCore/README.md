English | [繁體中文](README.zh-TW.md)

# CoreMesh.Result.AspNetCore

ASP.NET Core integration for `CoreMesh.Result`. Provides global exception handling, HTTP response conversion, and a consistent `ApiResponse` envelope.

## Registration

```csharp
builder.Services.AddCoreMeshHttp();
app.UseCoreMeshHttp();
```

Registers `ProblemDetails` and the following exception handlers (in order):

| Handler | Exception | HTTP Status |
|---------|-----------|-------------|
| `ValidationExceptionHandler` | `ValidationException` | 422 |
| `ConflictExceptionHandler` | `ConflictException` | 409 |
| `ForbiddenExceptionHandler` | `ForbiddenException` | 403 |
| `NotFoundExceptionHandler` | `NotFoundException` | 404 |
| `GlobalExceptionHandler` | Any unhandled exception | 400 / 401 / 500 |

## Converting Results to HTTP Responses

```csharp
// Non-generic Result → 200/204/400/404/...
result.ToHttpResult()

// Result<T> → ResultHttpResult<T> (includes OpenAPI metadata, no manual .Produces<T>() needed)
result.ToHttpResult()

// Other helpers
result.ToJson(statusCode: 201)
result.ToFile("application/pdf", "report.pdf")         // Result<byte[]>
result.ToFile("video/mp4", "video.mp4")                // Result<Stream>
result.ToServerSentEvents(eventType: "price")           // Result<IAsyncEnumerable<T>>
result.ToSignIn(principal, authenticationScheme: "...")
result.ToSignOut()
```

## Status → HTTP Mapping

| `ResultStatus` | HTTP |
|----------------|------|
| `Ok` | 200 |
| `Created` | 201 |
| `NoContent` | 204 |
| `BadRequest` | 400 |
| `Forbidden` | 403 |
| `NotFound` | 404 |
| `Invalid` | 422 |
| Other | 500 |

## ApiResponse Envelope

```json
// Success
{ "isSuccess": true, "code": "ok", "data": { ... } }

// Failure
{ "isSuccess": false, "code": "USER_001", "problem": { "status": 404, "detail": "..." } }

// Validation failure
{ "isSuccess": false, "code": "VALIDATION", "problem": { "status": 422, "errors": { ... } } }
```

`NoContent`, `ToFile()`, `ToServerSentEvents()`, `ToSignIn()`, and `ToSignOut()` do not use the envelope.

## GlobalExceptionHandler

- **Development**: returns the raw exception message
- **Production**: returns a generic message for non-`AppException` exceptions
- Always logs at `Error` level with `TraceId`
