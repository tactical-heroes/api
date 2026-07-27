using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;

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
            return FactionNotFound();
        }

        var updateResult = faction.UpdateDetails(
            name: command.Name,
            description: command.Description);

        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        await factionsRepository.UpdateAsync(
            aggregateRoot: faction,
            cancellationToken: cancellationToken);

        return Result.Success();
    }

    private static Result FactionNotFound()
    {
        return Result.Failure(error: Error.NotFound(message: "Faction was not found."));
    }
}
