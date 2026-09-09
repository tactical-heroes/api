using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.Domain.Units;

public sealed class Unit : AggregateRoot<UnitId>
{
    private Unit(
        UnitId id,
        UnitName name,
        UnitDescription description,
        UnitMorale morale,
        UnitLuck luck,
        FactionId factionId)
        : base(id)
    {
        Name = name;
        Description = description;
        Stats = null!;
        RangedAttack = null!;
        Morale = morale;
        Luck = luck;
        FactionId = factionId;
    }

    private Unit(
        UnitId id,
        UnitName name,
        UnitDescription description,
        UnitCombatStats stats,
        UnitRangedAttack rangedAttack,
        UnitMorale morale,
        UnitLuck luck,
        FactionId factionId)
        : this(
            id: id,
            name: name,
            description: description,
            morale: morale,
            luck: luck,
            factionId: factionId)
    {
        Stats = stats;
        RangedAttack = rangedAttack;
    }

    public UnitName Name { get; private set; }
    public UnitDescription Description { get; private set; }
    public UnitCombatStats Stats { get; private set; }
    public UnitRangedAttack RangedAttack { get; private set; }
    public UnitMorale Morale { get; private set; }
    public UnitLuck Luck { get; private set; }
    public FactionId FactionId { get; private set; }

    public static Unit Create(
        UnitName name,
        UnitDescription description,
        UnitCombatStats stats,
        UnitRangedAttack rangedAttack,
        UnitMorale morale,
        UnitLuck luck,
        FactionId factionId)
    {
        return new Unit(
            id: UnitId.New(),
            name: name,
            description: description,
            stats: stats,
            rangedAttack: rangedAttack,
            morale: morale,
            luck: luck,
            factionId: factionId);
    }

    public void Update(
        UnitName name,
        UnitDescription description,
        UnitCombatStats stats,
        UnitRangedAttack rangedAttack,
        UnitMorale morale,
        UnitLuck luck,
        FactionId factionId)
    {
        Name = name;
        Description = description;
        Stats = stats;
        RangedAttack = rangedAttack;
        Morale = morale;
        Luck = luck;
        FactionId = factionId;
    }
}
