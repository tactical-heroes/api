using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Domain.Heroes.ValueObjects;

public sealed class HeroLuckTests
{
    [Theory(DisplayName = "Hero luck should accept a value when value is within range")]
    [InlineData(HeroLuck.Minimum)]
    [InlineData(HeroLuck.Maximum)]
    public void Create_Should_ReturnLuck_When_ValueIsWithinRange(int value)
    {
        var result = HeroLuck.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
        result.Value.ToString().ShouldBe(value.ToString());
    }

    [Theory(DisplayName = "Hero luck should reject a value when value is outside range")]
    [InlineData(HeroLuck.Minimum - 1)]
    [InlineData(HeroLuck.Maximum + 1)]
    public void Create_Should_ReturnValidationFailure_When_ValueIsOutsideRange(
        int value)
    {
        var result = HeroLuck.Create(value);

        result.ShouldHaveSingleError(
                ErrorType.Validation,
                $"Hero luck must be between {HeroLuck.Minimum} and {HeroLuck.Maximum}.")
            .ShouldHaveField(nameof(HeroLuck));
    }

    [Fact(DisplayName = "Hero luck should return its value when converted to string")]
    public void ToString_Should_ReturnValue_When_ConvertedToString()
    {
        var luck = HeroLuck.Create(2).Value;

        var result = luck.ToString();

        result.ShouldBe("2");
    }
}
