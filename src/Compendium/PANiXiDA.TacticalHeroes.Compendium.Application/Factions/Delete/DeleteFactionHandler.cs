using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Delete;

public sealed class DeleteFactionHandler(IFactionsRepository factionsRepository)
    : ICommandHandler<DeleteFactionCommand, Result>, IRequireAuthorization, IRequireTransaction
{
    public static IReadOnlyCollection<string> AllPermissions { get; } = [Permissions.Compendium.Factions.Delete.Code];

    public async Task<Result> HandleAsync(
        DeleteFactionCommand command,
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

        factionsRepository.Delete(aggregateRoot: faction);

        return Result.Success();
    }
}
