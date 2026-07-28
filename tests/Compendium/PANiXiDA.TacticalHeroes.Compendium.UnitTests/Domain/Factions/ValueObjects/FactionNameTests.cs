using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Domain.Factions.ValueObjects;

public sealed class FactionNameTests
{
    [Fact(DisplayName = "Faction name should trim a valid value when name is valid")]
    public void Create_Should_TrimValue_When_NameIsValid()
    {
        var result = FactionName.Create("  Northern Alliance  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("Northern Alliance");
        result.Value.ToString().ShouldBe("Northern Alliance");
    }

    [Theory(DisplayName = "Faction name should reject an empty value when name is empty")]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_ReturnValidationFailure_When_NameIsEmpty(string value)
    {
        var result = FactionName.Create(value);

        result.ShouldHaveSingleError(
                ErrorType.Validation,
                "Faction name cannot be empty.")
            .ShouldHaveField(nameof(FactionName));
    }

    [Fact(DisplayName = "Faction name should reject a value over the maximum length when name is too long")]
    public void Create_Should_ReturnValidationFailure_When_NameIsTooLong()
    {
        var result = FactionName.Create(
            new string('a', FactionName.MaxLength + 1));

        result.ShouldHaveSingleError(
                ErrorType.Validation,
                $"Faction name cannot be longer than {FactionName.MaxLength} characters.")
            .ShouldHaveField(nameof(FactionName));
    }
}
