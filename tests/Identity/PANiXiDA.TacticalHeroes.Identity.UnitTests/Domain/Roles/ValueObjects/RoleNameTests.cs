using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.Roles.ValueObjects;

public sealed class RoleNameTests
{
    [Fact(DisplayName = "Role name should normalize a valid value when role name is valid")]
    public void Create_Should_NormalizeValue_When_RoleNameIsValid()
    {
        var result = RoleName.Create("  ADMIN  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("admin");
        result.Value.ToString().ShouldBe("admin");
    }

    [Theory(DisplayName = "Role name should reject an empty value when role name is empty")]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ReturnValidationFailure_When_RoleNameIsEmpty(string value)
    {
        var result = RoleName.Create(value);

        result.ShouldHaveSingleError(ErrorType.Validation, "Role name cannot be empty.")
            .ShouldHaveField(nameof(RoleName));
    }

    [Fact(DisplayName = "Role name should reject a value over the maximum length when role name is too long")]
    public void Create_Should_ReturnValidationFailure_When_RoleNameIsTooLong()
    {
        var result = RoleName.Create(new string('a', RoleName.MaxLength + 1));

        result.ShouldHaveSingleError(
                ErrorType.Validation,
                $"Role name cannot be longer than {RoleName.MaxLength} characters.")
            .ShouldHaveField(nameof(RoleName));
    }

    [Fact(DisplayName = "Role name should return its value when converted to string")]
    public void ToString_Should_ReturnValue_When_ConvertedToString()
    {
        var roleName = RoleName.Create("admin").Value;

        var result = roleName.ToString();

        result.ShouldBe(roleName.Value);
    }
}
