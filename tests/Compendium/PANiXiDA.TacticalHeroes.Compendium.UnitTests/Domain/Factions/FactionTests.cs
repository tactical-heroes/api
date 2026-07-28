using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Domain.Factions;

public sealed class FactionTests
{
    [Fact(DisplayName = "Faction should create valid normalized details when details are valid")]
    public void Create_Should_ReturnFaction_When_DetailsAreValid()
    {
        var result = Faction.Create(
            "  Northern Alliance  ",
            "  Defenders of the north.  ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.Value.ShouldNotBe(Guid.Empty);
        result.Value.Name.Value.ShouldBe("Northern Alliance");
        result.Value.Description.Value.ShouldBe("Defenders of the north.");
    }

    [Fact(DisplayName = "Faction should update valid details when values are valid")]
    public void Update_Should_ReplaceDetails_When_ValuesAreValid()
    {
        var faction = Faction.Create(
            "Northern Alliance",
            "Defenders of the north.").Value;

        var result = faction.Update(
            "  Southern Alliance  ",
            "  Defenders of the south.  ");

        result.IsSuccess.ShouldBeTrue();
        faction.Name.Value.ShouldBe("Southern Alliance");
        faction.Description.Value.ShouldBe("Defenders of the south.");
    }

    [Fact(DisplayName = "Faction should preserve details when value is invalid")]
    public void Update_Should_PreserveDetails_When_ValueIsInvalid()
    {
        var faction = Faction.Create(
            "Northern Alliance",
            "Defenders of the north.").Value;

        var result = faction.Update("", "");

        result.IsFailure.ShouldBeTrue();
        faction.Name.Value.ShouldBe("Northern Alliance");
        faction.Description.Value.ShouldBe("Defenders of the north.");
    }
}
