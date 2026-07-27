using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Domain.Factions.ValueObjects;

public sealed class FactionDescriptionTests
{
    [Fact(DisplayName = "Faction description should trim a valid value")]
    public void Create_Should_TrimValue_When_DescriptionIsValid()
    {
        var result = FactionDescription.Create("  Defenders of the north.  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("Defenders of the north.");
        result.Value.ToString().ShouldBe("Defenders of the north.");
    }

    [Theory(DisplayName = "Faction description should reject an empty value")]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ReturnValidationFailure_When_DescriptionIsEmpty(
        string value)
    {
        var result = FactionDescription.Create(value);

        result.ShouldHaveSingleError(
                ErrorType.Validation,
                "Faction description cannot be empty.")
            .ShouldHaveField(nameof(FactionDescription));
    }

    [Fact(DisplayName = "Faction description should reject a value over the maximum length")]
    public void Create_Should_ReturnValidationFailure_When_DescriptionIsTooLong()
    {
        var result = FactionDescription.Create(
            new string('a', FactionDescription.MaxLength + 1));

        result.ShouldHaveSingleError(
                ErrorType.Validation,
                $"Faction description cannot be longer than {FactionDescription.MaxLength} characters.")
            .ShouldHaveField(nameof(FactionDescription));
    }
}
