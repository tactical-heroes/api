using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Heroes.ValueObjects;
using PANiXiDA.TacticalHeroes.Battle.Domain.Factions;
using PANiXiDA.TacticalHeroes.Battle.Domain.Players;

using HeroDefinitionId = PANiXiDA.TacticalHeroes.Battle.Domain.Heroes.HeroId;

namespace PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Entities.Heroes;

public sealed class Hero : Entity<HeroId>
{
    private Hero(
        HeroId id,
        BattleId battleId,
        PlayerId ownerPlayerId,
        HeroDefinitionId definitionId,
        FactionId factionId)
        : base(id)
    {
        BattleId = battleId;
        OwnerPlayerId = ownerPlayerId;
        DefinitionId = definitionId;
        FactionId = factionId;
        Stats = null!;
    }

    public BattleId BattleId { get; private set; }
    public PlayerId OwnerPlayerId { get; private set; }
    public HeroDefinitionId DefinitionId { get; private set; }
    public FactionId FactionId { get; private set; }
    public HeroCombatStats Stats { get; private set; }

    public static Hero Create(
        BattleId battleId,
        PlayerId ownerPlayerId,
        HeroDefinitionId definitionId,
        FactionId factionId,
        HeroCombatStats stats)
    {
        return new Hero(
            id: HeroId.New(),
            battleId: battleId,
            ownerPlayerId: ownerPlayerId,
            definitionId: definitionId,
            factionId: factionId)
        { Stats = stats };
    }
}
