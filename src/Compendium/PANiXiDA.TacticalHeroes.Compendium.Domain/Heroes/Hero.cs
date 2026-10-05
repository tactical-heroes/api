using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes;

public sealed class Hero : AggregateRoot<HeroId>
{
    private Hero(
        HeroId id,
        HeroName name,
        HeroDescription description,
        HeroMorale morale,
        HeroLuck luck,
        FactionId factionId)
        : base(id)
    {
        Name = name;
        Description = description;
        Stats = null!;
        Morale = morale;
        Luck = luck;
        FactionId = factionId;
    }

    private Hero(
        HeroId id,
        HeroName name,
        HeroDescription description,
        HeroCombatStats stats,
        HeroMorale morale,
        HeroLuck luck,
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
    }

    public HeroName Name { get; private set; }
    public HeroDescription Description { get; private set; }
    public HeroCombatStats Stats { get; private set; }
    public HeroMorale Morale { get; private set; }
    public HeroLuck Luck { get; private set; }
    public FactionId FactionId { get; private set; }

    public static Hero Create(
        HeroName name,
        HeroDescription description,
        HeroCombatStats stats,
        HeroMorale morale,
        HeroLuck luck,
        FactionId factionId)
    {
        return new Hero(
            id: HeroId.New(),
            name: name,
            description: description,
            stats: stats,
            morale: morale,
            luck: luck,
            factionId: factionId);
    }

    public void Update(
        HeroName name,
        HeroDescription description,
        HeroCombatStats stats,
        HeroMorale morale,
        HeroLuck luck,
        FactionId factionId)
    {
        Name = name;
        Description = description;
        Stats = stats;
        Morale = morale;
        Luck = luck;
        FactionId = factionId;
    }
}
