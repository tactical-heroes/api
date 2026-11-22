using PANiXiDA.TacticalHeroes.Identity.Domain.Roles.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.UnitTests.Domain.Roles.ValueObjects;

public sealed class RoleNameTests
{
    [Theory(DisplayName = "Role name should trim whitespace and preserve casing when role name is valid")]
    [InlineData("  CustomManager  ", "CustomManager")]
    [InlineData("  custommanager  ", "custommanager")]
    [InlineData("  CUSTOMMANAGER  ", "CUSTOMMANAGER")]
    [InlineData("  Главный Модератор  ", "Главный Модератор")]
    public void Create_Should_TrimWhitespaceAndPreserveCasing_When_RoleNameIsValid(string value, string expected)
    {
        var result = RoleName.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(expected);
    }

    [Theory(DisplayName = "Role name should reject an empty value when role name is empty")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ReturnValidationFailure_When_RoleNameIsEmpty(string? value)
    {
        var result = RoleName.Create(value!);

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

    [Fact(DisplayName = "Role name should format its value when converted to string")]
    public void ToString_Should_FormatValue_When_ConvertedToString()
    {
        var roleName = RoleName.Create("CustomManager").Value;

        var result = roleName.ToString();

        result.ShouldBe("RoleName { Value = CustomManager }");
    }
}
