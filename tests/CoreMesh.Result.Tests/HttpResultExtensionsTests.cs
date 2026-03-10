using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;

namespace CoreMesh.Result.Tests;

public sealed class HttpResultExtensionsTests
{
    // ─── Result (non-generic) ────────────────────────────────────────────────

    [Fact]
    public void Result_Ok_ToHttpResult_Should_Return_Ok_With_Success_ApiResponse()
    {
        var result   = Result.Ok();
        var http     = result.ToHttpResult();
        var typed    = Assert.IsType<Ok<ApiResponse>>(http);

        Assert.NotNull(typed.Value);
        Assert.True(typed.Value.IsSuccess);
        Assert.Equal("ok", typed.Value.Code);
        Assert.Null(typed.Value.Problem);
    }

    [Fact]
    public void Result_Created_ToHttpResult_Should_Return_Created_With_Success_ApiResponse()
    {
        var result = Result.Created();
        var http   = result.ToHttpResult();
        var typed  = Assert.IsType<Created<ApiResponse>>(http);

        Assert.NotNull(typed.Value);
        Assert.True(typed.Value.IsSuccess);
        Assert.Equal("Created", typed.Value.Code);
        Assert.Null(typed.Value.Problem);
    }

    [Fact]
    public void Result_NoContent_ToHttpResult_Should_Return_NoContent()
    {
        var result = Result.NoContent();
        var http   = result.ToHttpResult();

        Assert.IsType<NoContent>(http);
    }

    [Fact]
    public void Result_BadRequest_ToHttpResult_Should_Return_BadRequest_With_Failure_ApiResponse()
    {
        var error  = new Error("BAD_INPUT", "Input is invalid");
        var result = Result.BadRequest(error);
        var http   = result.ToHttpResult();
        var typed  = Assert.IsType<BadRequest<ApiResponse>>(http);

        Assert.NotNull(typed.Value);
        Assert.False(typed.Value.IsSuccess);
        Assert.Equal("BAD_INPUT", typed.Value.Code);
        Assert.NotNull(typed.Value.Problem);
        Assert.Equal(400, typed.Value.Problem.Status);
        Assert.Equal("BAD_INPUT",    typed.Value.Problem.Title);
        Assert.Equal("Input is invalid", typed.Value.Problem.Detail);
    }

    [Fact]
    public void Result_NotFound_ToHttpResult_Should_Return_NotFound_With_Failure_ApiResponse()
    {
        var error  = new Error("NOT_FOUND", "Resource missing");
        var result = Result.NotFound(error);
        var http   = result.ToHttpResult();
        var typed  = Assert.IsType<NotFound<ApiResponse>>(http);

        Assert.NotNull(typed.Value);
        Assert.False(typed.Value.IsSuccess);
        Assert.Equal("NOT_FOUND", typed.Value.Code);
        Assert.NotNull(typed.Value.Problem);
        Assert.Equal(404, typed.Value.Problem.Status);
    }

    [Fact]
    public void Result_Forbidden_ToHttpResult_Should_Return_JsonHttpResult_With_StatusCode_403()
    {
        var error  = new Error("FORBIDDEN", "Access denied");
        var result = Result.Forbidden(error);
        var http   = result.ToHttpResult();
        var typed  = Assert.IsType<JsonHttpResult<ApiResponse>>(http);

        Assert.Equal(403, typed.StatusCode);
        Assert.NotNull(typed.Value);
        Assert.False(typed.Value.IsSuccess);
        Assert.Equal("FORBIDDEN", typed.Value.Code);
        Assert.NotNull(typed.Value.Problem);
        Assert.Equal(403, typed.Value.Problem.Status);
    }

    [Fact]
    public void Result_Invalid_SingleError_ToHttpResult_Should_Return_UnprocessableEntity_With_ProblemDetails()
    {
        var error  = new Error("INVALID", "Validation failed");
        var result = Result.Invalid(error);
        var http   = result.ToHttpResult();
        var typed  = Assert.IsType<UnprocessableEntity<ApiResponse>>(http);

        Assert.NotNull(typed.Value);
        Assert.False(typed.Value.IsSuccess);
        Assert.Equal("INVALID", typed.Value.Code);
        Assert.NotNull(typed.Value.Problem);
        Assert.IsNotType<ValidationProblemDetails>(typed.Value.Problem);
        Assert.Equal(422, typed.Value.Problem.Status);
    }

    [Fact]
    public void Result_Invalid_WithValidationErrors_ToHttpResult_Should_Return_UnprocessableEntity_With_ValidationProblemDetails()
    {
        var errors = new Dictionary<string, string[]>
        {
            { "Name",  ["Name is required"] },
            { "Email", ["Email is invalid"] }
        };
        var result = Result.Invalid(errors);
        var http   = result.ToHttpResult();
        var typed  = Assert.IsType<UnprocessableEntity<ApiResponse>>(http);

        Assert.NotNull(typed.Value);
        Assert.False(typed.Value.IsSuccess);
        var vpd = Assert.IsType<ValidationProblemDetails>(typed.Value.Problem);
        Assert.Equal(422, vpd.Status);
        Assert.True(vpd.Errors.ContainsKey("Name"));
        Assert.True(vpd.Errors.ContainsKey("Email"));
    }

    // ─── Result<T> ───────────────────────────────────────────────────────────
    // ToHttpResult() on Result<T> returns ResultHttpResult<T> (IEndpointMetadataProvider).
    // Outer type assertions use ResultHttpResult<T>; inner content is verified via
    // ToInnerHttpResult() which is accessible through InternalsVisibleTo.

    [Fact]
    public void ResultT_Ok_ToHttpResult_Should_Return_ResultHttpResult()
    {
        var result = Result<User>.Ok(new User(1, "Alice"));

        Assert.IsType<ResultHttpResult<User>>(result.ToHttpResult());
    }

    [Fact]
    public void ResultT_Ok_ToHttpResult_Inner_Should_Return_Ok_With_Data_In_ApiResponse()
    {
        var user   = new User(1, "Alice");
        var result = Result<User>.Ok(user);
        var typed  = Assert.IsType<Ok<ApiResponse<User>>>(result.ToInnerHttpResult());

        Assert.NotNull(typed.Value);
        Assert.True(typed.Value.IsSuccess);
        Assert.Equal("ok", typed.Value.Code);
        Assert.Null(typed.Value.Problem);
        Assert.Equal(user, typed.Value.Data);
    }

    [Fact]
    public void ResultT_Created_ToHttpResult_Inner_Should_Return_Created_With_Data_In_ApiResponse()
    {
        var user   = new User(2, "Bob");
        var result = Result<User>.Created(user);
        var typed  = Assert.IsType<Created<ApiResponse<User>>>(result.ToInnerHttpResult());

        Assert.NotNull(typed.Value);
        Assert.True(typed.Value.IsSuccess);
        Assert.Equal("created", typed.Value.Code);
        Assert.Equal(user, typed.Value.Data);
    }

    [Fact]
    public void ResultT_NoContent_ToHttpResult_Should_Return_NoContent()
    {
        // Result<T> has no public NoContent factory — use the non-generic Result instead,
        // which also exercises the same status branch.
        var result = Result.NoContent();
        var http   = result.ToHttpResult();

        Assert.IsType<NoContent>(http);
    }

    [Fact]
    public void ResultT_BadRequest_ToHttpResult_Inner_Should_Return_BadRequest_With_Failure_ApiResponse()
    {
        var error  = new Error("BAD", "Bad input");
        var result = Result<User>.BadRequest(error);
        var typed  = Assert.IsType<BadRequest<ApiResponse>>(result.ToInnerHttpResult());

        Assert.NotNull(typed.Value);
        Assert.False(typed.Value.IsSuccess);
        Assert.Equal("BAD", typed.Value.Code);
        Assert.NotNull(typed.Value.Problem);
        Assert.Equal(400, typed.Value.Problem.Status);
        Assert.Equal("Bad input", typed.Value.Problem.Detail);
    }

    [Fact]
    public void ResultT_NotFound_ToHttpResult_Inner_Should_Return_NotFound_With_Failure_ApiResponse()
    {
        var error  = new Error("NF", "Not found");
        var result = Result<User>.NotFound(error);
        var typed  = Assert.IsType<NotFound<ApiResponse>>(result.ToInnerHttpResult());

        Assert.NotNull(typed.Value);
        Assert.False(typed.Value.IsSuccess);
        Assert.Equal("NF", typed.Value.Code);
        Assert.NotNull(typed.Value.Problem);
        Assert.Equal(404, typed.Value.Problem.Status);
    }

    [Fact]
    public void ResultT_Forbidden_ToHttpResult_Inner_Should_Return_JsonHttpResult_With_StatusCode_403()
    {
        var error  = new Error("FORBIDDEN", "Access denied");
        var result = Result<User>.Forbidden(error);
        var typed  = Assert.IsType<JsonHttpResult<ApiResponse>>(result.ToInnerHttpResult());

        Assert.Equal(403, typed.StatusCode);
        Assert.NotNull(typed.Value);
        Assert.False(typed.Value.IsSuccess);
        Assert.Equal("FORBIDDEN", typed.Value.Code);
        Assert.NotNull(typed.Value.Problem);
        Assert.Equal(403, typed.Value.Problem.Status);
    }

    [Fact]
    public void ResultT_Invalid_SingleError_ToHttpResult_Inner_Should_Return_UnprocessableEntity_With_ProblemDetails()
    {
        var error  = new Error("INVALID", "Validation failed");
        var result = Result<User>.Invalid(error);
        var typed  = Assert.IsType<UnprocessableEntity<ApiResponse>>(result.ToInnerHttpResult());

        Assert.NotNull(typed.Value);
        Assert.False(typed.Value.IsSuccess);
        Assert.Equal("INVALID", typed.Value.Code);
        Assert.NotNull(typed.Value.Problem);
        Assert.IsNotType<ValidationProblemDetails>(typed.Value.Problem);
        Assert.Equal(422, typed.Value.Problem.Status);
    }

    [Fact]
    public void ResultT_Invalid_WithValidationErrors_ToHttpResult_Inner_Should_Return_UnprocessableEntity_With_ValidationProblemDetails()
    {
        var errors = new Dictionary<string, string[]>
        {
            { "Name",  ["Name is required"] },
            { "Email", ["Email is invalid"] }
        };
        var result = Result<User>.Invalid(errors);
        var typed  = Assert.IsType<UnprocessableEntity<ApiResponse>>(result.ToInnerHttpResult());

        Assert.NotNull(typed.Value);
        Assert.False(typed.Value.IsSuccess);
        var vpd = Assert.IsType<ValidationProblemDetails>(typed.Value.Problem);
        Assert.Equal(422, vpd.Status);
        Assert.True(vpd.Errors.ContainsKey("Name"));
        Assert.True(vpd.Errors.ContainsKey("Email"));
    }

    [Fact]
    public void ResultT_Invalid_WithValidationErrors_ToHttpResult_Inner_Should_Carry_ValidationErrors_In_VPD()
    {
        var errors = new Dictionary<string, string[]>
        {
            { "Name", ["Name is required", "Name must be at least 2 chars"] }
        };
        var result = Result<User>.Invalid(errors);
        var typed  = Assert.IsType<UnprocessableEntity<ApiResponse>>(result.ToInnerHttpResult());
        var vpd    = Assert.IsType<ValidationProblemDetails>(typed.Value!.Problem);

        Assert.Equal(["Name is required", "Name must be at least 2 chars"], vpd.Errors["Name"]);
    }

    // ─── ResultHttpResult<T> metadata ────────────────────────────────────────

    [Fact]
    public void ResultT_ToHttpResult_Should_Return_ResultHttpResult_That_Is_IResult()
    {
        var result = Result<User>.Ok(new User(1, "Alice"));

        Assert.IsAssignableFrom<IResult>(result.ToHttpResult());
    }

    [Fact]
    public void ResultHttpResult_PopulateMetadata_Should_Register_All_Response_Types()
    {
        var builder = new TestEndpointBuilder();
        ResultHttpResult<User>.PopulateMetadata(method: null!, builder);

        var statusCodes = builder.Metadata
            .OfType<IProducesResponseTypeMetadata>()
            .Select(m => m.StatusCode)
            .Order()
            .ToArray();

        Assert.Equal([200, 201, 204, 400, 403, 404, 422, 500], statusCodes);
    }

    [Fact]
    public void ResultHttpResult_PopulateMetadata_Should_Use_ApiResponseT_For_Success_Codes()
    {
        var builder = new TestEndpointBuilder();
        ResultHttpResult<User>.PopulateMetadata(method: null!, builder);

        var successMeta = builder.Metadata
            .OfType<IProducesResponseTypeMetadata>()
            .Where(m => m.StatusCode is 200 or 201);

        Assert.All(successMeta, m => Assert.Equal(typeof(ApiResponse<User>), m.Type));
    }

    [Fact]
    public void ResultHttpResult_PopulateMetadata_Should_Use_ApiResponse_For_Failure_Codes()
    {
        var builder = new TestEndpointBuilder();
        ResultHttpResult<User>.PopulateMetadata(method: null!, builder);

        var failureMeta = builder.Metadata
            .OfType<IProducesResponseTypeMetadata>()
            .Where(m => m.StatusCode is 400 or 403 or 404 or 422 or 500);

        Assert.All(failureMeta, m => Assert.Equal(typeof(ApiResponse), m.Type));
    }

    /// <summary>Minimal <see cref="EndpointBuilder"/> stub for metadata population tests.</summary>
    private sealed class TestEndpointBuilder : EndpointBuilder
    {
        public override Endpoint Build() => throw new NotSupportedException();
    }

    private sealed record User(int Id, string Name);
}
