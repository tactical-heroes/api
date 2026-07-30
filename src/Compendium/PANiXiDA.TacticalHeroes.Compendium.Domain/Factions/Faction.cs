using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;

public sealed class Faction : AggregateRoot<FactionId>
{
    private Faction(
        FactionId id,
        FactionName name,
        FactionDescription description)
        : base(id: id)
    {
        Name = name;
        Description = description;
    }

    public FactionName Name { get; private set; }
    public FactionDescription Description { get; private set; }

    public static Result<Faction> Create(
        string name,
        string description)
    {
        var nameResult = FactionName.Create(value: name);
        var descriptionResult = FactionDescription.Create(value: description);
        var validationResult = Result.Combine(
            results: [nameResult, descriptionResult]);

        return validationResult.IsFailure
            ? Result.Failure<Faction>(errors: validationResult.Errors)
            : Result.Success(
                value: new Faction(
                    id: FactionId.New(),
                    name: nameResult.Value,
                    description: descriptionResult.Value));
    }

    public Result Update(
        string name,
        string description)
    {
        var nameResult = FactionName.Create(value: name);
        var descriptionResult = FactionDescription.Create(value: description);
        var validationResult = Result.Combine(
            results: [nameResult, descriptionResult]);

        if (validationResult.IsFailure)
        {
            return Result.Failure(errors: validationResult.Errors);
        }

        Name = nameResult.Value;
        Description = descriptionResult.Value;

        return Result.Success();
    }
}
