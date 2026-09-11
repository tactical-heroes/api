using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Domain.Heroes.ValueObjects;

public sealed class HeroMoraleTests
{
    [Theory(DisplayName = "Hero morale should accept a value when value is within range")]
    [InlineData(HeroMorale.Minimum)]
    [InlineData(HeroMorale.Maximum)]
    public void Create_Should_ReturnMorale_When_ValueIsWithinRange(int value)
    {
        var result = HeroMorale.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(value);
        result.Value.ToString().ShouldBe(value.ToString());
    }

    [Theory(DisplayName = "Hero morale should reject a value when value is outside range")]
    [InlineData(HeroMorale.Minimum - 1)]
    [InlineData(HeroMorale.Maximum + 1)]
    public void Create_Should_ReturnValidationFailure_When_ValueIsOutsideRange(
        int value)
    {
        var result = HeroMorale.Create(value);

        result.ShouldHaveSingleError(
                ErrorType.Validation,
                $"Hero morale must be between {HeroMorale.Minimum} and {HeroMorale.Maximum}.")
            .ShouldHaveField(nameof(HeroMorale));
    }

    [Fact(DisplayName = "Hero morale should return its value when converted to string")]
    public void ToString_Should_ReturnValue_When_ConvertedToString()
    {
        var morale = HeroMorale.Create(4).Value;

        var result = morale.ToString();

        result.ShouldBe("4");
    }
}
