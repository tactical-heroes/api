using PANiXiDA.TacticalHeroes.Compendium.Domain.Units.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Domain.Units.ValueObjects;

public sealed class UnitNameTests
{
    [Fact(DisplayName = "Unit name should trim a valid value when name is valid")]
    public void Create_Should_TrimValue_When_NameIsValid()
    {
        var result = UnitName.Create("  Archer  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("Archer");
        result.Value.ToString().ShouldBe("Archer");
    }

    [Theory(DisplayName = "Unit name should reject an empty value when name is empty")]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ReturnValidationFailure_When_NameIsEmpty(string value)
    {
        var result = UnitName.Create(value);

        result.ShouldHaveSingleError(
                ErrorType.Validation,
                "Unit name cannot be empty.")
            .ShouldHaveField(nameof(UnitName));
    }

    [Fact(DisplayName = "Unit name should reject a value over the maximum length when name is too long")]
    public void Create_Should_ReturnValidationFailure_When_NameIsTooLong()
    {
        var result = UnitName.Create(
            new string('a', UnitName.MaxLength + 1));

        result.ShouldHaveSingleError(
                ErrorType.Validation,
                $"Unit name cannot be longer than {UnitName.MaxLength} characters.")
            .ShouldHaveField(nameof(UnitName));
    }

    [Fact(DisplayName = "Unit name should return its value when converted to string")]
    public void ToString_Should_ReturnValue_When_ConvertedToString()
    {
        var name = UnitName.Create("Archer").Value;

        var result = name.ToString();

        result.ShouldBe(name.Value);
    }
}
