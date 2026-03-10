namespace CoreMesh.Result.Tests;

public sealed class ResultExtensionsTests
{
    // ─── Result (non-generic) factory methods ────────────────────────────────

    [Fact]
    public void Result_Ok_Should_Return_Success_With_Ok_Status()
    {
        var result = Result.Ok();

        Assert.True(result.IsSuccess);
        Assert.Equal(ResultStatus.Ok, result.Status);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void Result_Created_Should_Return_Success_With_Created_Status()
    {
        var result = Result.Created();

        Assert.True(result.IsSuccess);
        Assert.Equal(ResultStatus.Created, result.Status);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void Result_NoContent_Should_Return_Success_With_NoContent_Status()
    {
        var result = Result.NoContent();

        Assert.True(result.IsSuccess);
        Assert.Equal(ResultStatus.NoContent, result.Status);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void Result_BadRequest_Should_Return_Failure_With_BadRequest_Status()
    {
        var error  = new Error("BAD", "Bad request");
        var result = Result.BadRequest(error);

        Assert.True(result.IsFailure);
        Assert.Equal(ResultStatus.BadRequest, result.Status);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Result_NotFound_Should_Return_Failure_With_NotFound_Status()
    {
        var error  = new Error("NF", "Not found");
        var result = Result.NotFound(error);

        Assert.True(result.IsFailure);
        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Result_Forbidden_Should_Return_Failure_With_Forbidden_Status()
    {
        var error  = new Error("FORBIDDEN", "Access denied");
        var result = Result.Forbidden(error);

        Assert.True(result.IsFailure);
        Assert.Equal(ResultStatus.Forbidden, result.Status);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Result_Invalid_SingleError_Should_Return_Failure_With_Invalid_Status()
    {
        var error  = new Error("INVALID", "Validation failed");
        var result = Result.Invalid(error);

        Assert.True(result.IsFailure);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Result_Invalid_IDictionary_Should_Populate_ValidationErrors()
    {
        var errors = new Dictionary<string, string[]>
        {
            { "Name",  ["Name is required"] },
            { "Email", ["Email is invalid"] }
        };

        var result = Result.Invalid(errors);

        Assert.True(result.IsFailure);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Equal(2, result.ValidationErrors.Count);
        Assert.Equal(["Name is required"], result.ValidationErrors["Name"]);
        Assert.Equal(["Email is invalid"], result.ValidationErrors["Email"]);
    }

    [Fact]
    public void Result_Invalid_IDictionary_Should_Not_Share_Reference_With_Input()
    {
        var errors = new Dictionary<string, string[]>
        {
            { "Name", ["Name is required"] }
        };

        var result = Result.Invalid(errors);

        errors["Name"] = ["mutated"];

        Assert.Equal(["Name is required"], result.ValidationErrors["Name"]);
    }

    // ─── Result<T> factory methods ───────────────────────────────────────────

    [Fact]
    public void ResultT_Ok_Should_Return_Success_With_Ok_Status_And_Data()
    {
        var user   = new User(1, "Alice");
        var result = Result<User>.Ok(user);

        Assert.True(result.IsSuccess);
        Assert.Equal(ResultStatus.Ok, result.Status);
        Assert.Equal(user, result.Data);
    }

    [Fact]
    public void ResultT_Created_Should_Return_Success_With_Created_Status_And_Data()
    {
        var user   = new User(2, "Bob");
        var result = Result<User>.Created(user);

        Assert.True(result.IsSuccess);
        Assert.Equal(ResultStatus.Created, result.Status);
        Assert.Equal(user, result.Data);
    }

    [Fact]
    public void ResultT_BadRequest_Should_Return_Failure_With_BadRequest_Status()
    {
        var error  = new Error("BAD", "Bad request");
        var result = Result<User>.BadRequest(error);

        Assert.True(result.IsFailure);
        Assert.Equal(ResultStatus.BadRequest, result.Status);
        Assert.Equal(error, result.Error);
        Assert.Null(result.Data);
    }

    [Fact]
    public void ResultT_NotFound_Should_Return_Failure_With_NotFound_Status()
    {
        var error  = new Error("NF", "Not found");
        var result = Result<User>.NotFound(error);

        Assert.True(result.IsFailure);
        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.Equal(error, result.Error);
        Assert.Null(result.Data);
    }

    [Fact]
    public void ResultT_Forbidden_Should_Return_Failure_With_Forbidden_Status()
    {
        var error  = new Error("FORBIDDEN", "Access denied");
        var result = Result<User>.Forbidden(error);

        Assert.True(result.IsFailure);
        Assert.Equal(ResultStatus.Forbidden, result.Status);
        Assert.Equal(error, result.Error);
        Assert.Null(result.Data);
    }

    [Fact]
    public void ResultT_Invalid_SingleError_Should_Return_Failure_With_Invalid_Status()
    {
        var error  = new Error("INVALID", "Validation failed");
        var result = Result<User>.Invalid(error);

        Assert.True(result.IsFailure);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Equal(error, result.Error);
        Assert.Null(result.Data);
    }

    [Fact]
    public void ResultT_Invalid_IDictionary_Should_Populate_ValidationErrors()
    {
        var errors = new Dictionary<string, string[]>
        {
            { "Name",  ["Name is required"] },
            { "Email", ["Email is invalid", "Email is required"] }
        };

        var result = Result<User>.Invalid(errors);

        Assert.True(result.IsFailure);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Equal(2, result.ValidationErrors.Count);
        Assert.Equal(["Name is required"],                      result.ValidationErrors["Name"]);
        Assert.Equal(["Email is invalid", "Email is required"], result.ValidationErrors["Email"]);
    }

    [Fact]
    public void ResultT_Invalid_IDictionary_Should_Not_Share_Reference_With_Input()
    {
        var errors = new Dictionary<string, string[]>
        {
            { "Name", ["Name is required"] }
        };

        var result = Result<User>.Invalid(errors);

        errors["Name"] = ["mutated"];

        Assert.Equal(["Name is required"], result.ValidationErrors["Name"]);
    }

    [Fact]
    public void ResultT_Invalid_IDictionary_ValidationErrors_Should_BeEmpty_When_Created_Via_SingleError()
    {
        var result = Result<User>.Invalid(new Error("INVALID", "Validation failed"));

        Assert.Empty(result.ValidationErrors);
    }

    private sealed record User(int Id, string Name);
}
