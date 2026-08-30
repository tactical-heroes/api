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

    public static Result<Hero> Create(HeroAttributes attributes)
    {
        var validationResult = ValidateAttributes(attributes);

        return validationResult.IsFailure
            ? Result.Failure<Hero>(errors: validationResult.Errors)
            : Result.Success(
                value: new Hero(
                    id: HeroId.New(),
                    name: validationResult.Value.Name,
                    description: validationResult.Value.Description,
                    stats: validationResult.Value.Stats,
                    morale: validationResult.Value.Morale,
                    luck: validationResult.Value.Luck,
                    factionId: validationResult.Value.FactionId));
    }

    public Result Update(HeroAttributes attributes)
    {
        var validationResult = ValidateAttributes(attributes);

        if (validationResult.IsFailure)
        {
            return Result.Failure(errors: validationResult.Errors);
        }

        Name = validationResult.Value.Name;
        Description = validationResult.Value.Description;
        Stats = validationResult.Value.Stats;
        Morale = validationResult.Value.Morale;
        Luck = validationResult.Value.Luck;
        FactionId = validationResult.Value.FactionId;

        return Result.Success();
    }

    private static Result<ValidatedAttributes> ValidateAttributes(HeroAttributes attributes)
    {
        var nameResult = HeroName.Create(value: attributes.Name);
        var descriptionResult = HeroDescription.Create(value: attributes.Description);
        var statsResult = HeroCombatStats.Create(
            attack: attributes.Attack,
            defense: attributes.Defense,
            minimumDamage: attributes.MinimumDamage,
            maximumDamage: attributes.MaximumDamage,
            initiative: attributes.Initiative);
        var moraleResult = HeroMorale.Create(value: attributes.Morale);
        var luckResult = HeroLuck.Create(value: attributes.Luck);
        var factionIdResult = FactionId.Create(value: attributes.FactionId);
        var validationResult = Result.Combine(
            nameResult,
            descriptionResult,
            statsResult,
            moraleResult,
            luckResult,
            factionIdResult);

        return validationResult.IsFailure
            ? Result.Failure<ValidatedAttributes>(errors: validationResult.Errors)
            : Result.Success(
                value: new ValidatedAttributes(
                    Name: nameResult.Value,
                    Description: descriptionResult.Value,
                    Stats: statsResult.Value,
                    Morale: moraleResult.Value,
                    Luck: luckResult.Value,
                    FactionId: factionIdResult.Value));
    }

    private sealed record ValidatedAttributes(
        HeroName Name,
        HeroDescription Description,
        HeroCombatStats Stats,
        HeroMorale Morale,
        HeroLuck Luck,
        FactionId FactionId);
}
