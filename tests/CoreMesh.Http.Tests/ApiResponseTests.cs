using System.Text.Json;
using CoreMesh.Http.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CoreMesh.Http.Tests;

public sealed class ApiResponseTests
{
    [Fact]
    public void OnSuccess_Should_Serialize_Expected_Envelope()
    {
        var response = ApiResponse<object>.OnSuccess(new { Name = "Book" });

        var json = JsonSerializer.Serialize(response);

        Assert.Contains("\"success\":true", json);
        Assert.Contains("\"code\":\"ok\"", json);
        Assert.Contains("\"data\":", json);

        Assert.DoesNotContain("\"problem\":", json);
        Assert.DoesNotContain("\"traceId\":", json);
    }

    [Fact]
    public void OnFailure_Should_Serialize_Expected_Envelope()
    {
        var problem = new ProblemDetails
        {
            Title = "Validation failed",
            Status = 400,
            Detail = "One or more validation errors occurred."
        };
        problem.Extensions["traceId"] = "trace-123";

        var response = ApiResponse.OnFailure(problem, "validation_error");

        var json = JsonSerializer.Serialize(response);

        Assert.Contains("\"success\":false", json);
        Assert.Contains("\"code\":\"validation_error\"", json);
        Assert.Contains("\"traceId\":\"trace-123\"", json);
        Assert.Contains("\"problem\":", json);

        Assert.DoesNotContain("\"data\":", json);
    }
}
