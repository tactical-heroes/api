using PANiXiDA.TacticalHeroes.Compendium.Application.Units.Create;
using PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetDetails;
using PANiXiDA.TacticalHeroes.Compendium.Application.Units.Update;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units;

namespace PANiXiDA.TacticalHeroes.Compendium.UnitTests.Units;

internal static class UnitTestData
{
    internal static Faction CreateFaction()
    {
        return Faction.Create(
            "Northern Alliance",
            "Defenders of the north.").Value;
    }

    internal static Unit CreateUnit(Faction faction)
    {
        return Unit.Create(
            name: "Archer",
            description: "A disciplined ranged unit.",
            attack: 8,
            defense: 4,
            health: 12,
            minimumDamage: 3,
            maximumDamage: 5,
            initiative: 10.5,
            speed: 6,
            shots: 12,
            rangedAttackRange: 8,
            morale: 2,
            luck: 1,
            factionId: faction.Id.Value).Value;
    }

    internal static CreateUnitCommand CreateCommand(Guid factionId)
    {
        return new CreateUnitCommand(
            Name: "Archer",
            Description: "A disciplined ranged unit.",
            Attack: 8,
            Defense: 4,
            Health: 12,
            MinimumDamage: 3,
            MaximumDamage: 5,
            Initiative: 10.5,
            Speed: 6,
            Shots: 12,
            RangedAttackRange: 8,
            Morale: 2,
            Luck: 1,
            FactionId: factionId);
    }

    internal static UpdateUnitCommand CreateUpdateCommand(
        Guid unitId,
        Guid factionId)
    {
        return new UpdateUnitCommand(
            Id: unitId,
            Name: "Marksman",
            Description: "An elite ranged unit.",
            Attack: 10,
            Defense: 5,
            Health: 14,
            MinimumDamage: 4,
            MaximumDamage: 7,
            Initiative: 11.5,
            Speed: 7,
            Shots: 16,
            RangedAttackRange: 10,
            Morale: 3,
            Luck: 2,
            FactionId: factionId);
    }

    internal static UnitDetailsReadModel CreateDetailsReadModel(Guid factionId)
    {
        return new UnitDetailsReadModel(
            Id: Guid.CreateVersion7(),
            Name: "Archer",
            Description: "A disciplined ranged unit.",
            Attack: 8,
            Defense: 4,
            Health: 12,
            MinimumDamage: 3,
            MaximumDamage: 5,
            Initiative: 10.5,
            Speed: 6,
            Shots: 12,
            RangedAttackRange: 8,
            Morale: 2,
            Luck: 1,
            FactionId: factionId);
    }
}
