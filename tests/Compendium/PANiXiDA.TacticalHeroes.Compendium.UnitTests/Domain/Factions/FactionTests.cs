using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Domain.Factions;

public sealed class FactionTests
{
    [Fact(DisplayName = "Faction should create valid normalized details when details are valid")]
    public void Create_Should_ReturnFaction_When_DetailsAreValid()
    {
        var result = Faction.Create(
            name: FactionName.Create(value: "  Northern Alliance  ").Value,
            description: FactionDescription.Create(value: "  Defenders of the north.  ").Value);

        result.Id.Value.ShouldNotBe(Guid.Empty);
        result.Name.Value.ShouldBe("Northern Alliance");
        result.Description.Value.ShouldBe("Defenders of the north.");
    }

    [Fact(DisplayName = "Faction should update valid details when values are valid")]
    public void Update_Should_ReplaceDetails_When_ValuesAreValid()
    {
        var faction = Faction.Create(
            name: FactionName.Create(value: "Northern Alliance").Value,
            description: FactionDescription.Create(value: "Defenders of the north.").Value);

        faction.Update(
            name: FactionName.Create(value: "  Southern Alliance  ").Value,
            description: FactionDescription.Create(value: "  Defenders of the south.  ").Value);

        faction.Name.Value.ShouldBe("Southern Alliance");
        faction.Description.Value.ShouldBe("Defenders of the south.");
    }

}
