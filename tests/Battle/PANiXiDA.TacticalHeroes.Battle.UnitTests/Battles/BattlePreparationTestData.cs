using PANiXiDA.TacticalHeroes.Battle.Application.ReadModels;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Events;

namespace PANiXiDA.TacticalHeroes.Battle.UnitTests.Battles;

internal static class BattlePreparationTestData
{
    internal static BattleCreationRequested CreateRequest()
    {
        return new BattleCreationRequested(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7());
    }

    internal static UnitReadModel CreateUnit()
    {
        return new UnitReadModel(
            Id: Guid.CreateVersion7(),
            Name: "Unit",
            Description: "Unit",
            Attack: 8,
            Defense: 6,
            Health: 20,
            MinimumDamage: 2,
            MaximumDamage: 5,
            Initiative: 12,
            Speed: 4,
            Shots: 10,
            RangedAttackRange: 5,
            Morale: 1,
            Luck: 2,
            FactionId: Guid.CreateVersion7());
    }

    internal static HeroReadModel CreateHero()
    {
        return new HeroReadModel(
            Id: Guid.CreateVersion7(),
            Name: "Hero",
            Description: "Hero",
            Attack: 4,
            Defense: 5,
            MinimumDamage: 2,
            MaximumDamage: 3,
            Initiative: 10,
            Morale: 2,
            Luck: 1,
            FactionId: Guid.CreateVersion7());
    }
}
