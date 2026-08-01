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
        Morale = morale;
        Luck = luck;
        FactionId = factionId;
    }

    private Unit(
        UnitId id,
        UnitName name,
        UnitDescription description,
        UnitCombatStats stats,
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
    }

    public UnitName Name { get; private set; }
    public UnitDescription Description { get; private set; }
    public UnitCombatStats Stats { get; private set; }
    public UnitMorale Morale { get; private set; }
    public UnitLuck Luck { get; private set; }
    public FactionId FactionId { get; private set; }

    public static Result<Unit> Create(
        string name,
        string description,
        int attack,
        int defense,
        int health,
        int minimumDamage,
        int maximumDamage,
        double initiative,
        int speed,
        int? shots,
        int? rangedAttackRange,
        int morale,
        int luck,
        Guid factionId)
    {
        var nameResult = UnitName.Create(value: name);
        var descriptionResult = UnitDescription.Create(value: description);
        var statsResult = UnitCombatStats.Create(
            attack: attack,
            defense: defense,
            health: health,
            minimumDamage: minimumDamage,
            maximumDamage: maximumDamage,
            initiative: initiative,
            speed: speed,
            shots: shots,
            rangedAttackRange: rangedAttackRange);
        var moraleResult = UnitMorale.Create(value: morale);
        var luckResult = UnitLuck.Create(value: luck);
        var factionIdResult = FactionId.Create(value: factionId);
        var validationResult = Result.Combine(
            nameResult,
            descriptionResult,
            statsResult,
            moraleResult,
            luckResult,
            factionIdResult);

        return validationResult.IsFailure
            ? Result.Failure<Unit>(errors: validationResult.Errors)
            : Result.Success(
                value: new Unit(
                    id: UnitId.New(),
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
        int health,
        int minimumDamage,
        int maximumDamage,
        double initiative,
        int speed,
        int? shots,
        int? rangedAttackRange,
        int morale,
        int luck,
        Guid factionId)
    {
        var nameResult = UnitName.Create(value: name);
        var descriptionResult = UnitDescription.Create(value: description);
        var statsResult = UnitCombatStats.Create(
            attack: attack,
            defense: defense,
            health: health,
            minimumDamage: minimumDamage,
            maximumDamage: maximumDamage,
            initiative: initiative,
            speed: speed,
            shots: shots,
            rangedAttackRange: rangedAttackRange);
        var moraleResult = UnitMorale.Create(value: morale);
        var luckResult = UnitLuck.Create(value: luck);
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
