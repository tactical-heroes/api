using PANiXiDA.TacticalHeroes.Battle.Domain.Battles;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Heroes;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Heroes.ValueObjects;
using PANiXiDA.TacticalHeroes.Battle.Domain.Factions;
using PANiXiDA.TacticalHeroes.Battle.Domain.Players;

using HeroDefinitionId = PANiXiDA.TacticalHeroes.Battle.Domain.Heroes.HeroId;

namespace PANiXiDA.TacticalHeroes.Battle.UnitTests.Domain.Battles.Entities.Heroes;

public sealed class HeroTests
{
    [Fact(DisplayName = "Hero should capture its owner and immutable stats when selected")]
    public void Create_Should_CaptureOwnerAndStats_When_Selected()
    {
        var battleId = BattleId.New();
        var owner = PlayerId.New();
        var definition = HeroDefinitionId.New();
        var faction = FactionId.New();
        var stats = HeroCombatStats.Create(
            attack: 3,
            defense: 4,
            minimumDamage: 2,
            maximumDamage: 4,
            initiative: 10,
            morale: 1,
            luck: 1).Value;

        var hero = Hero.Create(
            battleId: battleId,
            ownerPlayerId: owner,
            definitionId: definition,
            factionId: faction,
            stats: stats);

        hero.BattleId.ShouldBe(battleId);
        hero.OwnerPlayerId.ShouldBe(owner);
        hero.DefinitionId.ShouldBe(definition);
        hero.FactionId.ShouldBe(faction);
        hero.Stats.ShouldBe(stats);
    }
}
