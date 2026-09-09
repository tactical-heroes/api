using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Units.Create;

public sealed class CreateUnitHandler(
    IUnitsRepository unitsRepository,
    IFactionsRepository factionsRepository)
    : ICommandHandler<CreateUnitCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(
        CreateUnitCommand command,
        CancellationToken cancellationToken)
    {
        var nameResult = UnitName.Create(value: command.Name);
        var descriptionResult = UnitDescription.Create(value: command.Description);
        var statsResult = command.ToCombatStats();
        var moraleResult = UnitMorale.Create(value: command.Morale);
        var luckResult = UnitLuck.Create(value: command.Luck);
        var factionIdResult = FactionId.Create(value: command.FactionId);
        var validationResult = Result.Combine(
            nameResult,
            descriptionResult,
            statsResult,
            moraleResult,
            luckResult,
            factionIdResult);

        if (validationResult.IsFailure)
        {
            return Result.Failure<Guid>(errors: validationResult.Errors);
        }

        var unit = Unit.Create(
            name: nameResult.Value,
            description: descriptionResult.Value,
            stats: statsResult.Value,
            morale: moraleResult.Value,
            luck: luckResult.Value,
            factionId: factionIdResult.Value);

        var faction = await factionsRepository.GetByIdAsync(
            id: unit.FactionId,
            cancellationToken: cancellationToken);

        if (faction is null)
        {
            return Result.Failure<Guid>(
                error: Error.NotFound(message: "Faction was not found."));
        }

        await unitsRepository.AddAsync(
            aggregateRoot: unit,
            cancellationToken: cancellationToken);

        return Result.Success(value: unit.Id.Value);
    }
}
