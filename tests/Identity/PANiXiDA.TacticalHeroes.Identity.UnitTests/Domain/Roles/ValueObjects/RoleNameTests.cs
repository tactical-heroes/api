using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.Roles.ValueObjects;

public sealed class RoleNameTests
{
    [Fact(DisplayName = "Role name should normalize a valid value")]
    public void Create_Should_NormalizeValue_When_RoleNameIsValid()
    {
        var result = RoleName.Create("  ADMIN  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("admin");
        result.Value.ToString().ShouldBe("admin");
    }

    [Theory(DisplayName = "Role name should reject an empty value")]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ReturnValidationFailure_When_RoleNameIsEmpty(string value)
    {
        var result = RoleName.Create(value);

        result.ShouldHaveSingleError(ErrorType.Validation, "Role name cannot be empty.")
            .ShouldHaveField(nameof(RoleName));
    }

    [Fact(DisplayName = "Role name should reject a value over the maximum length")]
    public void Create_Should_ReturnValidationFailure_When_RoleNameIsTooLong()
    {
        var result = RoleName.Create(new string('a', RoleName.MaxLength + 1));

        result.ShouldHaveSingleError(
                ErrorType.Validation,
                $"Role name cannot be longer than {RoleName.MaxLength} characters.")
            .ShouldHaveField(nameof(RoleName));
    }
}
