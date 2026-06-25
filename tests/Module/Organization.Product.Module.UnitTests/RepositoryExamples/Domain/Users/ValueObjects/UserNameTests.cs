using Organization.Product.Module.Domain.Users.ValueObjects;

namespace Organization.Product.Module.UnitTests.Domain.Users.ValueObjects;

public sealed class UserNameTests
{
    [Fact(DisplayName = "Create should trim name when value has surrounding whitespace")]
    public void Create_Should_Trim_Name_When_Value_Has_Surrounding_Whitespace()
    {
        var value = "  John Doe  ";

        var result = UserName.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("John Doe");
        result.Value.ToString().ShouldBe("John Doe");
    }

    [Fact(DisplayName = "Create should return failure when name is empty")]
    public void Create_Should_Return_Failure_When_Name_Is_Empty()
    {
        var value = "   ";

        var result = UserName.Create(value);

        var error = result.ShouldHaveSingleError(
            ErrorType.Validation,
            "User name cannot be empty.");

        error.ShouldHaveField(nameof(UserName));
    }

    [Fact(DisplayName = "Create should return failure when name is too long")]
    public void Create_Should_Return_Failure_When_Name_Is_Too_Long()
    {
        var value = new string('a', 201);

        var result = UserName.Create(value);

        var error = result.ShouldHaveSingleError(
            ErrorType.Validation,
            "User name cannot be longer than 200 characters.");

        error.ShouldHaveField(nameof(UserName));
    }

    [Fact(DisplayName = "Equals should compare by value when names are equal")]
    public void Equals_Should_Compare_By_Value_When_Names_Are_Equal()
    {
        var first = UserName.Create("John Doe").Value;
        var second = UserName.Create("John Doe").Value;

        first.ShouldBe(second);
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }
}
