using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Domain.Heroes.ValueObjects;

public sealed class HeroDescriptionTests
{
    [Fact(DisplayName = "Hero description should trim a valid value when description is valid")]
    public void Create_Should_TrimValue_When_DescriptionIsValid()
    {
        var result = HeroDescription.Create("  A seasoned northern commander.  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("A seasoned northern commander.");
        result.Value.ToString().ShouldBe("A seasoned northern commander.");
    }

    [Theory(DisplayName = "Hero description should reject an empty value when description is empty")]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ReturnValidationFailure_When_DescriptionIsEmpty(
        string value)
    {
        var result = HeroDescription.Create(value);

        result.ShouldHaveSingleError(
                ErrorType.Validation,
                "Hero description cannot be empty.")
            .ShouldHaveField(nameof(HeroDescription));
    }

    [Fact(DisplayName = "Hero description should reject a value over the maximum length when description is too long")]
    public void Create_Should_ReturnValidationFailure_When_DescriptionIsTooLong()
    {
        var result = HeroDescription.Create(
            new string('a', HeroDescription.MaxLength + 1));

        result.ShouldHaveSingleError(
                ErrorType.Validation,
                $"Hero description cannot be longer than {HeroDescription.MaxLength} characters.")
            .ShouldHaveField(nameof(HeroDescription));
    }

    [Fact(DisplayName = "Hero description should return its value when converted to string")]
    public void ToString_Should_ReturnValue_When_ConvertedToString()
    {
        var description = HeroDescription.Create(
            "A seasoned northern commander.").Value;

        var result = description.ToString();

        result.ShouldBe(description.Value);
    }
}
