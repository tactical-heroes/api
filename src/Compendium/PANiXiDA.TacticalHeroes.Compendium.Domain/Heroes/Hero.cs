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

    public static Result<Hero> Create(
        string name,
        string description,
        int attack,
        int defense,
        int minimumDamage,
        int maximumDamage,
        double initiative,
        int morale,
        int luck,
        Guid factionId)
    {
        var nameResult = HeroName.Create(value: name);
        var descriptionResult = HeroDescription.Create(value: description);
        var statsResult = HeroCombatStats.Create(
            attack: attack,
            defense: defense,
            minimumDamage: minimumDamage,
            maximumDamage: maximumDamage,
            initiative: initiative);
        var moraleResult = HeroMorale.Create(value: morale);
        var luckResult = HeroLuck.Create(value: luck);
        var factionIdResult = FactionId.Create(value: factionId);
        var validationResult = Result.Combine(
            nameResult,
            descriptionResult,
            statsResult,
            moraleResult,
            luckResult,
            factionIdResult);

        return validationResult.IsFailure
            ? Result.Failure<Hero>(errors: validationResult.Errors)
            : Result.Success(
                value: new Hero(
                    id: HeroId.New(),
                    name: nameResult.Value,
                    description: descriptionResult.Value,
                    stats: statsResult.Value,
                    morale: moraleResult.Value,
                    luck: luckResult.Value,
                    factionId: factionIdResult.Value));
    }

    public Result Update(
        string name,
        string description,
        int attack,
        int defense,
        int minimumDamage,
        int maximumDamage,
        double initiative,
        int morale,
        int luck,
        Guid factionId)
    {
        var nameResult = HeroName.Create(value: name);
        var descriptionResult = HeroDescription.Create(value: description);
        var statsResult = HeroCombatStats.Create(
            attack: attack,
            defense: defense,
            minimumDamage: minimumDamage,
            maximumDamage: maximumDamage,
            initiative: initiative);
        var moraleResult = HeroMorale.Create(value: morale);
        var luckResult = HeroLuck.Create(value: luck);
        var factionIdResult = FactionId.Create(value: factionId);
        var validationResult = Result.Combine(
            nameResult,
            descriptionResult,
            statsResult,
            moraleResult,
            luckResult,
            factionIdResult);

        if (validationResult.IsFailure)
        {
            return Result.Failure(errors: validationResult.Errors);
        }

        Name = nameResult.Value;
        Description = descriptionResult.Value;
        Stats = statsResult.Value;
        Morale = moraleResult.Value;
        Luck = luckResult.Value;
        FactionId = factionIdResult.Value;

        return Result.Success();
    }
}
