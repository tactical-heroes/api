using PANiXiDA.TacticalHeroes.Compendium.Domain.Units.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Domain.Units.ValueObjects;

public sealed class UnitMoraleTests
{
    [Theory(DisplayName = "Unit morale should accept a value when value is within range")]
    [InlineData(UnitMorale.Minimum)]
    [InlineData(UnitMorale.Maximum)]
    public void Create_Should_ReturnMorale_When_ValueIsWithinRange(int value)
    {
        var result = UnitMorale.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
        result.Value.ToString().ShouldBe(value.ToString());
    }

    [Theory(DisplayName = "Unit morale should reject a value when value is outside range")]
    [InlineData(UnitMorale.Minimum - 1)]
    [InlineData(UnitMorale.Maximum + 1)]
    public void Create_Should_ReturnValidationFailure_When_ValueIsOutsideRange(
        int value)
    {
        var result = UnitMorale.Create(value);

        result.ShouldHaveSingleError(
                ErrorType.Validation,
                $"Unit morale must be between {UnitMorale.Minimum} and {UnitMorale.Maximum}.")
            .ShouldHaveField(nameof(UnitMorale));
    }

    [Fact(DisplayName = "Unit morale should return its value when converted to string")]
    public void ToString_Should_ReturnValue_When_ConvertedToString()
    {
        var morale = UnitMorale.Create(2).Value;

        var result = morale.ToString();

        result.ShouldBe("2");
    }
}
