namespace CoreMesh.Result.Tests;

public sealed class ResultTests
{
    // ─── Result (non-generic) ────────────────────────────────────────────────

    [Fact]
    public void Result_Constructor_Should_Throw_When_SuccessStatus_With_NonNoneError()
    {
        // The constructor is protected; invoke it via a concrete subclass exposed only in this test.
        var error = new Error("ERR", "Some error");

        Assert.Throws<ArgumentException>(() => new ExposedResult(ResultStatus.Ok,        error));
        Assert.Throws<ArgumentException>(() => new ExposedResult(ResultStatus.Created,   error));
        Assert.Throws<ArgumentException>(() => new ExposedResult(ResultStatus.NoContent, error));
    }

    [Fact]
    public void Result_Constructor_Should_Throw_When_FailureStatus_With_ErrorNone()
    {
        Assert.Throws<ArgumentException>(() => new ExposedResult(ResultStatus.BadRequest, Error.None));
        Assert.Throws<ArgumentException>(() => new ExposedResult(ResultStatus.NotFound,   Error.None));
        Assert.Throws<ArgumentException>(() => new ExposedResult(ResultStatus.Forbidden,  Error.None));
        Assert.Throws<ArgumentException>(() => new ExposedResult(ResultStatus.Invalid,    Error.None));
    }

    // Thin subclass used only to exercise the protected constructor in tests.
    private sealed record ExposedResult(ResultStatus S, Error E) : Result(S, E);

    [Fact]
    public void Result_Ok_Should_Have_ErrorNone()
    {
        var result = Result.Ok();

        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void Result_IsSuccess_Should_BeTrue_For_Ok()
    {
        var result = Result.Ok();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
    }

    [Fact]
    public void Result_IsSuccess_Should_BeTrue_For_Created()
    {
        var result = Result.Created();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
    }

    [Fact]
    public void Result_IsSuccess_Should_BeTrue_For_NoContent()
    {
        var result = Result.NoContent();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
    }

    [Fact]
    public void Result_IsFailure_Should_BeTrue_For_BadRequest()
    {
        var result = Result.BadRequest(new Error("ERR", "desc"));

        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Result_Status_Should_Match_For_Ok()
    {
        Assert.Equal(ResultStatus.Ok, Result.Ok().Status);
    }

    [Fact]
    public void Result_Status_Should_Match_For_Created()
    {
        Assert.Equal(ResultStatus.Created, Result.Created().Status);
    }

    [Fact]
    public void Result_Status_Should_Match_For_NoContent()
    {
        Assert.Equal(ResultStatus.NoContent, Result.NoContent().Status);
    }

    [Fact]
    public void Result_Status_Should_Match_For_NotFound()
    {
        Assert.Equal(ResultStatus.NotFound, Result.NotFound(new Error("NF", "not found")).Status);
    }

    [Fact]
    public void Result_Error_Should_Be_ErrorNone_For_Success()
    {
        var result = Result.Ok();

        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void Result_Error_Should_Carry_Provided_Error_For_Failure()
    {
        var error  = new Error("NOT_FOUND", "Resource was not found");
        var result = Result.NotFound(error);

        Assert.Equal(error.Code,        result.Error.Code);
        Assert.Equal(error.Description, result.Error.Description);
    }

    [Fact]
    public void Result_ValidationErrors_Should_BeEmpty_By_Default()
    {
        var result = Result.Ok();

        Assert.Empty(result.ValidationErrors);
    }

    [Fact]
    public void Result_Invalid_IDictionary_Should_Populate_ValidationErrors()
    {
        var errors = new Dictionary<string, string[]>
        {
            { "Name",  ["Name is required"] },
            { "Email", ["Email is invalid", "Email is required"] }
        };

        var result = Result.Invalid(errors);

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.False(result.IsSuccess);
        Assert.Equal(2, result.ValidationErrors.Count);
        Assert.Equal(["Name is required"],                      result.ValidationErrors["Name"]);
        Assert.Equal(["Email is invalid", "Email is required"], result.ValidationErrors["Email"]);
    }

    [Fact]
    public void Result_ImplicitOperator_From_Error_Should_Create_BadRequest()
    {
        var error  = new Error("ERR_CODE", "Something went wrong");
        Result result = error;

        Assert.Equal(ResultStatus.BadRequest, result.Status);
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    // ─── Error ───────────────────────────────────────────────────────────────

    [Fact]
    public void Error_None_Should_Have_Empty_Code_And_Description()
    {
        Assert.Equal(string.Empty, Error.None.Code);
        Assert.Equal(string.Empty, Error.None.Description);
    }

    [Fact]
    public void Error_None_Should_Equal_Another_ErrorNone()
    {
        Assert.Equal(Error.None, Error.None);
        Assert.Equal(new Error(string.Empty, string.Empty), Error.None);
    }

    // ─── Result<T> ───────────────────────────────────────────────────────────

    [Fact]
    public void ResultT_Data_Should_Be_Set_For_Success()
    {
        var user   = new User(1, "Alice");
        var result = Result<User>.Ok(user);

        Assert.True(result.IsSuccess);
        Assert.Equal(user, result.Data);
    }

    [Fact]
    public void ResultT_Data_Should_Be_Null_For_Failure()
    {
        var result = Result<User>.BadRequest(new Error("ERR", "error"));

        Assert.True(result.IsFailure);
        Assert.Null(result.Data);
    }

    [Fact]
    public void ResultT_IsSuccess_Should_BeTrue_For_Ok()
    {
        var result = Result<User>.Ok(new User(1, "Alice"));

        Assert.True(result.IsSuccess);
        Assert.Equal(ResultStatus.Ok, result.Status);
    }

    [Fact]
    public void ResultT_IsSuccess_Should_BeTrue_For_Created()
    {
        var result = Result<User>.Created(new User(2, "Bob"));

        Assert.True(result.IsSuccess);
        Assert.Equal(ResultStatus.Created, result.Status);
    }

    [Fact]
    public void ResultT_IsFailure_Should_BeTrue_For_NotFound()
    {
        var result = Result<User>.NotFound(new Error("NF", "not found"));

        Assert.True(result.IsFailure);
        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    [Fact]
    public void ResultT_Error_Should_Match_Provided_Error()
    {
        var error  = new Error("FORBIDDEN", "Access denied");
        var result = Result<User>.Forbidden(error);

        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void ResultT_ValidationErrors_Should_BePopulated_From_IDictionary()
    {
        var errors = new Dictionary<string, string[]>
        {
            { "Name", ["Name is required"] }
        };

        var result = Result<User>.Invalid(errors);

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Equal(["Name is required"], result.ValidationErrors["Name"]);
    }

    [Fact]
    public void ResultT_ImplicitOperator_From_Value_Should_Create_Ok()
    {
        var user = new User(42, "Carol");
        Result<User> result = user;

        Assert.Equal(ResultStatus.Ok, result.Status);
        Assert.True(result.IsSuccess);
        Assert.Equal(user, result.Data);
    }

    [Fact]
    public void ResultT_ImplicitOperator_From_Error_Should_Create_BadRequest()
    {
        var error = new Error("BAD", "Bad input");
        Result<User> result = error;

        Assert.Equal(ResultStatus.BadRequest, result.Status);
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void ResultT_Should_Be_Assignable_To_NonGeneric_Result()
    {
        Result<User> typed = new User(1, "Dave");
        Result result = typed;

        Assert.True(result.IsSuccess);
        Assert.Equal(ResultStatus.Ok, result.Status);
    }

    private sealed record User(int Id, string Name);
}
