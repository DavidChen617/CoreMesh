[繁體中文](README.zh-TW.md) | English

# CoreMesh.Http

`CoreMesh.Http` provides a consistent API response envelope and global exception handling for ASP.NET Core.

## Goals

- Keep a consistent response shape for success and error responses
- Use `ProblemDetails` for error payloads
- Provide ASP.NET Core global exception handlers via DI
- Keep the module HTTP-focused (no dependency on `CoreMesh.Validation`)

## Response Envelope

`CoreMesh.Http` uses a unified response envelope:

- `success`
- `data`
- `problem`
- `code`

### Success Example

```json
{
  "success": true,
  "data": {
    "id": 123,
    "name": "Book"
  },
  "code": "ok"
}
```

### Error Example

```json
{
  "success": false,
  "problem": {
    "title": "Validation failed",
    "status": 400,
    "detail": "One or more validation errors occurred.",
    "instance": "/products",
    "errors": {
      "traceId": "trace-123",
      "Name": ["Name is required."]
    }
  },
  "code": "validation_error"
}
```

Validation errors are stored in `ProblemDetails.Extensions["errors"]`.

## API Response Types

- `ApiResponse`
- `ApiResponse<T>`

### Create Success Response

```csharp
using CoreMesh.Http.Responses;

var response = ApiResponse<ProductDto>.OnSuccess(data, "ok");
return Results.Json(response);
```

### Create Failure Response

```csharp
using CoreMesh.Http.Responses;
using Microsoft.AspNetCore.Mvc;

var problem = new ProblemDetails
{
    Title = "Not Found",
    Status = StatusCodes.Status404NotFound,
    Detail = "Product was not found.",
    Instance = httpContext.Request.Path
};
problem.Extensions["traceId"] = httpContext.TraceIdentifier;

var response = ApiResponse.OnFailure(problem, code: "not_found");
return Results.Json(response, statusCode: StatusCodes.Status404NotFound);
```

## Global Exception Handling

`CoreMesh.Http` registers ASP.NET Core `IExceptionHandler` implementations:

- `ValidationExceptionHandler`
- `ConflictExceptionHandler`
- `ForbiddenExceptionHandler`
- `NotFoundExceptionHandler`
- `GlobalExceptionHandler` (fallback)

## ASP.NET Core Registration

```csharp
using CoreMesh.Http.Extensions;

builder.Services.AddCoreMeshHttp();

var app = builder.Build();

app.UseCoreMeshHttp();
```

## Built-in Exceptions

`CoreMesh.Http.Exceptions` currently includes:

- `AppException`
- `BadRequestException`
- `ConflictException`
- `ConcurrencyException`
- `ForbiddenException`
- `NotFoundException`
- `ExternalServiceException`
- `ValidationException`

## Design Notes

- `CoreMesh.Http` intentionally does **not** depend on `CoreMesh.Validation`
- `ValidationException` in this module carries HTTP-ready validation error data (`Dictionary<string, string[]>`)
- The module does **not** auto-wrap successful `Results.Ok(...)` globally; success wrapping is explicit

## Current Scope

Included:

- API response envelope models (`ApiResponse`, `ApiResponse<T>`)
- ASP.NET Core exception handlers
- DI extensions (`AddCoreMeshHttp`, `UseCoreMeshHttp`)
- `ProblemDetails`-based error responses

Not included yet:

- Automatic wrapping of all successful responses via middleware
- Endpoint helpers (e.g., `ApiResults.Ok(...)`)
- XML documentation comments
- Package metadata / NuGet README integration
