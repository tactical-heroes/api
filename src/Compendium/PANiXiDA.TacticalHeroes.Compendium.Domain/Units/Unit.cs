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

    public static Result<Unit> Create(UnitAttributes attributes)
    {
        var nameResult = UnitName.Create(value: attributes.Name);
        var descriptionResult = UnitDescription.Create(value: attributes.Description);
        var statsResult = UnitCombatStats.Create(input: attributes.CombatStats);
        var moraleResult = UnitMorale.Create(value: attributes.Morale);
        var luckResult = UnitLuck.Create(value: attributes.Luck);
        var factionIdResult = FactionId.Create(value: attributes.FactionId);
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

    public Result Update(UnitAttributes attributes)
    {
        var nameResult = UnitName.Create(value: attributes.Name);
        var descriptionResult = UnitDescription.Create(value: attributes.Description);
        var statsResult = UnitCombatStats.Create(input: attributes.CombatStats);
        var moraleResult = UnitMorale.Create(value: attributes.Morale);
        var luckResult = UnitLuck.Create(value: attributes.Luck);
        var factionIdResult = FactionId.Create(value: attributes.FactionId);
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
