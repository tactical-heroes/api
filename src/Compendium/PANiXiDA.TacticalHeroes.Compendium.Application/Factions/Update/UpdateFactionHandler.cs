using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Update;

public sealed class UpdateFactionHandler(IFactionsRepository factionsRepository)
    : ICommandHandler<UpdateFactionCommand, Result>
{
    public async Task<Result> HandleAsync(
        UpdateFactionCommand command,
        CancellationToken cancellationToken)
    {
        var idResult = FactionId.Create(value: command.Id);

        if (idResult.IsFailure)
        {
            return Result.Failure(errors: idResult.Errors);
        }

        var faction = await factionsRepository.GetByIdAsync(
            id: idResult.Value,
            cancellationToken: cancellationToken);

        if (faction is null)
        {
            return Result.Failure(
                error: Error.NotFound(message: "Faction was not found."));
        }

        var nameResult = FactionName.Create(value: command.Name);
        var descriptionResult = FactionDescription.Create(value: command.Description);
        var validationResult = Result.Combine(
            nameResult,
            descriptionResult);

        if (validationResult.IsFailure)
        {
            return Result.Failure(errors: validationResult.Errors);
        }

        faction.Update(
            name: nameResult.Value,
            description: descriptionResult.Value);

        await factionsRepository.UpdateAsync(
            aggregateRoot: faction,
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}
