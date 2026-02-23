[English](README.md) | 繁體中文

# CoreMesh.Http

`CoreMesh.Http` 提供 ASP.NET Core 的統一 API 回傳格式（response envelope）與全域例外處理能力。

## 目標

- 成功與錯誤回應維持一致的回傳結構
- 錯誤內容使用 `ProblemDetails`
- 透過 DI 註冊 ASP.NET Core 全域 exception handlers
- 保持模組聚焦於 HTTP（不依賴 `CoreMesh.Validation`）

## 統一回傳格式

`CoreMesh.Http` 使用以下 envelope：

- `success`
- `data`
- `problem`
- `code`

### 成功回應範例

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

### 錯誤回應範例

```json
{
  "success": false,
  "problem": {
    "title": "Validation failed",
    "status": 400,
    "detail": "One or more validation errors occurred.",
    "instance": "/products",
    "errors": {
      "Name": ["Name is required."]
    },
    "traceId": "trace-123"
  },
  "code": "validation_error"
}
```

驗證錯誤會放在 `ProblemDetails.Extensions["errors"]`。

## API 回應型別

- `ApiResponse`
- `ApiResponse<T>`

### 建立成功回應

```csharp
using CoreMesh.Http.Responses;

var response = ApiResponse<ProductDto>.OnSuccess(data, "ok");
return Results.Json(response);
```

### 建立失敗回應

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

var response = ApiResponse.OnFailure(problem, code: "not_found", traceId: httpContext.TraceIdentifier);
return Results.Json(response, statusCode: StatusCodes.Status404NotFound);
```

## 全域例外處理

`CoreMesh.Http` 內建 ASP.NET Core `IExceptionHandler`：

- `ValidationExceptionHandler`
- `ConflictExceptionHandler`
- `ForbiddenExceptionHandler`
- `NotFoundExceptionHandler`
- `GlobalExceptionHandler`（fallback）

## ASP.NET Core 註冊方式

```csharp
using CoreMesh.Http.Extensions;

builder.Services.AddCoreMeshHttp();

var app = builder.Build();

app.UseCoreMeshHttp();
```

## 內建例外型別

`CoreMesh.Http.Exceptions` 目前包含：

- `AppException`
- `BadRequestException`
- `ConflictException`
- `ConcurrencyException`
- `ForbiddenException`
- `NotFoundException`
- `ExternalServiceException`
- `ValidationException`

## 設計說明

- `CoreMesh.Http` 刻意不依賴 `CoreMesh.Validation`
- 本模組的 `ValidationException` 攜帶 HTTP 可直接輸出的驗證錯誤資料（`Dictionary<string, string[]>`）
- 目前不做全域自動包裝成功回應（例如自動攔截 `Results.Ok(...)`）；成功回應包裝採顯式方式

## 目前範圍

已包含：

- API 回應 envelope 模型（`ApiResponse`, `ApiResponse<T>`）
- ASP.NET Core exception handlers
- DI 擴充方法（`AddCoreMeshHttp`, `UseCoreMeshHttp`）
- 基於 `ProblemDetails` 的錯誤輸出

尚未包含：

- middleware 自動包裝所有成功回應
- endpoint helper（例如 `ApiResults.Ok(...)`）
- XML 文件註解
- NuGet 套件 metadata / README 打包設定
