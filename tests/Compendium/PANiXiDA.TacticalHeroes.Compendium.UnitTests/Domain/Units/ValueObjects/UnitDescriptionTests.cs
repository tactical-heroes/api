using PANiXiDA.TacticalHeroes.Compendium.Domain.Units.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Domain.Units.ValueObjects;

public sealed class UnitDescriptionTests
{
    [Fact(DisplayName = "Unit description should trim a valid value when description is valid")]
    public void Create_Should_TrimValue_When_DescriptionIsValid()
    {
        var result = UnitDescription.Create("  A disciplined ranged unit.  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("A disciplined ranged unit.");
        result.Value.ToString().ShouldBe("A disciplined ranged unit.");
    }

    [Theory(DisplayName = "Unit description should reject an empty value when description is empty")]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ReturnValidationFailure_When_DescriptionIsEmpty(
        string value)
    {
        var result = UnitDescription.Create(value);

        result.ShouldHaveSingleError(
                ErrorType.Validation,
                "Unit description cannot be empty.")
            .ShouldHaveField(nameof(UnitDescription));
    }

    [Fact(DisplayName = "Unit description should reject a value over the maximum length when description is too long")]
    public void Create_Should_ReturnValidationFailure_When_DescriptionIsTooLong()
    {
        var result = UnitDescription.Create(
            new string('a', UnitDescription.MaxLength + 1));

        result.ShouldHaveSingleError(
                ErrorType.Validation,
                $"Unit description cannot be longer than {UnitDescription.MaxLength} characters.")
            .ShouldHaveField(nameof(UnitDescription));
    }

    [Fact(DisplayName = "Unit description should return its value when converted to string")]
    public void ToString_Should_ReturnValue_When_ConvertedToString()
    {
        var description = UnitDescription.Create("A disciplined ranged unit.").Value;

        var result = description.ToString();

        result.ShouldBe(description.Value);
    }
}
