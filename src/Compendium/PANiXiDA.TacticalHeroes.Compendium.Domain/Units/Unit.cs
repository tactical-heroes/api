using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.Domain.Units;

public sealed class Unit : AggregateRoot<UnitId>
{
    private Unit()
        : base(id: default)
    {
        Name = null!;
        Description = null!;
        Stats = null!;
        Morale = null!;
        Luck = null!;
    }

    private Unit(
        UnitId id,
        UnitName name,
        UnitDescription description,
        UnitCombatStats stats,
        UnitMorale morale,
        UnitLuck luck,
        FactionId factionId)
        : base(id)
    {
        Name = name;
        Description = description;
        Stats = stats;
        Morale = morale;
        Luck = luck;
        FactionId = factionId;
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
        return CreateValidated(
            id: UnitId.New(),
            name: name,
            description: description,
            attack: attack,
            defense: defense,
            health: health,
            minimumDamage: minimumDamage,
            maximumDamage: maximumDamage,
            initiative: initiative,
            speed: speed,
            shots: shots,
            rangedAttackRange: rangedAttackRange,
            morale: morale,
            luck: luck,
            factionId: factionId);
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
        var candidateResult = CreateValidated(
            id: Id,
            name: name,
            description: description,
            attack: attack,
            defense: defense,
            health: health,
            minimumDamage: minimumDamage,
            maximumDamage: maximumDamage,
            initiative: initiative,
            speed: speed,
            shots: shots,
            rangedAttackRange: rangedAttackRange,
            morale: morale,
            luck: luck,
            factionId: factionId);

        if (candidateResult.IsFailure)
        {
            return Result.Failure(errors: candidateResult.Errors);
        }

        var candidate = candidateResult.Value;
        Name = candidate.Name;
        Description = candidate.Description;
        Stats = candidate.Stats;
        Morale = candidate.Morale;
        Luck = candidate.Luck;
        FactionId = candidate.FactionId;

        return Result.Success();
    }

    private static Result<Unit> CreateValidated(
        UnitId id,
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
                    id: id,
                    name: nameResult.Value,
                    description: descriptionResult.Value,
                    stats: statsResult.Value,
                    morale: moraleResult.Value,
                    luck: luckResult.Value,
                    factionId: factionIdResult.Value));
    }
}
