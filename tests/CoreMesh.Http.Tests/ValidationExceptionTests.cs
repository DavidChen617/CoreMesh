using CoreMesh.Http.Exceptions;

namespace CoreMesh.Http.Tests;

public sealed class ValidationExceptionTests
{
    [Fact]
    public void Ctor_With_Field_And_Error_Should_Create_Single_Error()
    {
        var ex = new ValidationException("Name", "Name is required.");

        Assert.Equal("One or more validation errors occurred.", ex.Message);
        Assert.True(ex.Errors.ContainsKey("Name"));
        Assert.Single(ex.Errors["Name"]);
        Assert.Equal("Name is required.", ex.Errors["Name"][0]);
    }

    [Fact]
    public void Ctor_With_Failures_Should_Group_By_PropertyName()
    {
        var failures = new[]
        {
            new ValidationErrorItem("Name", "Name is required."),
            new ValidationErrorItem("Name", "Name length must be between 2 and 50."),
            new ValidationErrorItem("Price", "Price must be greater than zero.")
        };

        var ex = new ValidationException(failures);

        Assert.Equal(2, ex.Errors.Count);

        Assert.True(ex.Errors.ContainsKey("Name"));
        Assert.Equal(2, ex.Errors["Name"].Length);

        Assert.True(ex.Errors.ContainsKey("Price"));
        Assert.Single(ex.Errors["Price"]);
    }

    [Fact]
    public void Ctor_With_Dictionary_Should_Keep_Reference_Data()
    {
        var errors = new Dictionary<string, string[]>
        {
            ["Name"] = ["Name is required."]
        };

        var ex = new ValidationException(errors);

        Assert.Same(errors, ex.Errors);
    }
}
