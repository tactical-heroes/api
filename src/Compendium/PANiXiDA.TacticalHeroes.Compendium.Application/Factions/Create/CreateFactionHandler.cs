using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Create;

public sealed class CreateFactionHandler(IFactionsRepository factionsRepository)
    : ICommandHandler<CreateFactionCommand, Result<Guid>>, IRequireAuthorization
{
    public static IReadOnlyCollection<string> AllPermissions { get; } = [Permissions.Compendium.Factions.Create.Code];

    public async Task<Result<Guid>> HandleAsync(
        CreateFactionCommand command,
        CancellationToken cancellationToken)
    {
        var nameResult = FactionName.Create(value: command.Name);
        var descriptionResult = FactionDescription.Create(value: command.Description);
        var validationResult = Result.Combine(
            nameResult,
            descriptionResult);

        if (validationResult.IsFailure)
        {
            return Result.Failure<Guid>(errors: validationResult.Errors);
        }

        var faction = Faction.Create(
            name: nameResult.Value,
            description: descriptionResult.Value);

        await factionsRepository.AddAsync(
            aggregateRoot: faction,
            cancellationToken: cancellationToken);

        return Result.Success(value: faction.Id.Value);
    }
}
