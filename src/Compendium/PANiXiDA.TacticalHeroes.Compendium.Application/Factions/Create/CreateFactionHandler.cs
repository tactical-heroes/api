using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Create;

public sealed class CreateFactionHandler(IFactionsRepository factionsRepository)
    : ICommandHandler<CreateFactionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(
        CreateFactionCommand command,
        CancellationToken cancellationToken)
    {
        var factionResult = Faction.Create(
            name: command.Name,
            description: command.Description);

        if (factionResult.IsFailure)
        {
            return Result.Failure<Guid>(errors: factionResult.Errors);
        }

        await factionsRepository.AddAsync(
            aggregateRoot: factionResult.Value,
            cancellationToken: cancellationToken);

        return Result.Success(value: factionResult.Value.Id.Value);
    }
}
