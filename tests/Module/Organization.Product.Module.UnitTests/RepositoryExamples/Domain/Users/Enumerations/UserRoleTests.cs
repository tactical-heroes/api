using Organization.Product.Module.Domain.Users.Enumerations;

namespace Organization.Product.Module.UnitTests.Domain.Users.Enumerations;

public sealed class UserRoleTests
{
    [Theory(DisplayName = "Create should return role when value is declared")]
    [InlineData("User")]
    [InlineData(" Admin ")]
    [InlineData("Moderator")]
    public void Create_Should_Return_Role_When_Value_Is_Declared(string role)
    {
        var result = UserRole.Create(role);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe(role.Trim());
    }

    [Fact(DisplayName = "Create should return failure when role is empty")]
    public void Create_Should_Return_Failure_When_Role_Is_Empty()
    {
        var value = " ";

        var result = UserRole.Create(value);

        var error = result.ShouldHaveSingleError(
            ErrorType.Validation,
            "User role cannot be empty.");

        error.ShouldHaveField(nameof(UserRole));
    }

    [Fact(DisplayName = "Create should return failure when role is not declared")]
    public void Create_Should_Return_Failure_When_Role_Is_Not_Declared()
    {
        var value = "Unknown";

        var result = UserRole.Create(value);

        var error = result.ShouldHaveSingleError(
            ErrorType.Validation,
            "User role 'Unknown' is invalid.");

        error.ShouldHaveField(nameof(UserRole));
    }
}
