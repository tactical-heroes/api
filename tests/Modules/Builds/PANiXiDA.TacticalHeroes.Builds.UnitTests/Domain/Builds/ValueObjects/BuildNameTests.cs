using PANiXiDA.TacticalHeroes.Builds.Domain.Builds.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Builds.UnitTests.Domain.Builds.ValueObjects;

public sealed class BuildNameTests
{
    [Fact(DisplayName = "Build name should trim a valid value when name is valid")]
    public void Create_Should_TrimValue_When_NameIsValid()
    {
        var result = BuildName.Create("  Northern Alliance  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("Northern Alliance");
    }

    [Theory(DisplayName = "Build name should reject an empty value when name is empty")]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ReturnValidationFailure_When_NameIsEmpty(string value)
    {
        var result = BuildName.Create(value);

        result.ShouldHaveSingleError(
            ErrorType.Validation,
            "Build name cannot be empty.")
            .ShouldHaveField(nameof(BuildName));
    }

    [Fact(DisplayName = "Build name should reject a value over the maximum length when name is too long")]
    public void Create_Should_ReturnValidationFailure_When_NameIsTooLong()
    {
        var result = BuildName.Create(
            new string('a', BuildName.MaxLength + 1));

        result.ShouldHaveSingleError(
            ErrorType.Validation,
            $"Build name cannot be longer than {BuildName.MaxLength} characters.")
            .ShouldHaveField(nameof(BuildName));
    }

    [Fact(DisplayName = "Build name should format its value when converted to string")]
    public void ToString_Should_FormatValue_When_ConvertedToString()
    {
        var name = BuildName.Create("Northern Alliance").Value;

        var result = name.ToString();

        result.ShouldBe("BuildName { Value = Northern Alliance }");
    }
}
