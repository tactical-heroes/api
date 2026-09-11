using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Domain.Heroes.ValueObjects;

public sealed class HeroNameTests
{
    [Fact(DisplayName = "Hero name should trim a valid value when name is valid")]
    public void Create_Should_TrimValue_When_NameIsValid()
    {
        var result = HeroName.Create("  Orrin  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("Orrin");
        result.Value.ToString().ShouldBe("Orrin");
    }

    [Theory(DisplayName = "Hero name should reject an empty value when name is empty")]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ReturnValidationFailure_When_NameIsEmpty(string value)
    {
        var result = HeroName.Create(value);

        result.ShouldHaveSingleError(
                ErrorType.Validation,
                "Hero name cannot be empty.")
            .ShouldHaveField(nameof(HeroName));
    }

    [Fact(DisplayName = "Hero name should reject a value over the maximum length when name is too long")]
    public void Create_Should_ReturnValidationFailure_When_NameIsTooLong()
    {
        var result = HeroName.Create(
            new string('a', HeroName.MaxLength + 1));

        result.ShouldHaveSingleError(
                ErrorType.Validation,
                $"Hero name cannot be longer than {HeroName.MaxLength} characters.")
            .ShouldHaveField(nameof(HeroName));
    }

    [Fact(DisplayName = "Hero name should return its value when converted to string")]
    public void ToString_Should_ReturnValue_When_ConvertedToString()
    {
        var name = HeroName.Create("Orrin").Value;

        var result = name.ToString();

        result.ShouldBe(name.Value);
    }
}
