using PANiXiDA.TacticalHeroes.Compendium.Domain.Units.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Domain.Units.ValueObjects;

public sealed class UnitLuckTests
{
    [Theory(DisplayName = "Unit luck should accept a value when value is within range")]
    [InlineData(UnitLuck.Minimum)]
    [InlineData(UnitLuck.Maximum)]
    public void Create_Should_ReturnLuck_When_ValueIsWithinRange(int value)
    {
        var result = UnitLuck.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
        result.Value.ToString().ShouldBe(value.ToString());
    }

    [Theory(DisplayName = "Unit luck should reject a value when value is outside range")]
    [InlineData(UnitLuck.Minimum - 1)]
    [InlineData(UnitLuck.Maximum + 1)]
    public void Create_Should_ReturnValidationFailure_When_ValueIsOutsideRange(
        int value)
    {
        var result = UnitLuck.Create(value);

        result.ShouldHaveSingleError(
                ErrorType.Validation,
                $"Unit luck must be between {UnitLuck.Minimum} and {UnitLuck.Maximum}.")
            .ShouldHaveField(nameof(UnitLuck));
    }

    [Fact(DisplayName = "Unit luck should return its value when converted to string")]
    public void ToString_Should_ReturnValue_When_ConvertedToString()
    {
        var luck = UnitLuck.Create(1).Value;

        var result = luck.ToString();

        result.ShouldBe("1");
    }
}
