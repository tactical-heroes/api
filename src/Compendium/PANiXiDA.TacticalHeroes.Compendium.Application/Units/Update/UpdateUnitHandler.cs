using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Units.Update;

public sealed class UpdateUnitHandler(
    IUnitsRepository unitsRepository,
    IFactionsRepository factionsRepository)
    : ICommandHandler<UpdateUnitCommand, Result>
{
    public async Task<Result> HandleAsync(
        UpdateUnitCommand command,
        CancellationToken cancellationToken)
    {
        var idResult = UnitId.Create(value: command.Id);

        if (idResult.IsFailure)
        {
            return Result.Failure(errors: idResult.Errors);
        }

        var unit = await unitsRepository.GetByIdAsync(
            id: idResult.Value,
            cancellationToken: cancellationToken);

        if (unit is null)
        {
            return Result.Failure(
                error: Error.NotFound(message: "Unit was not found."));
        }

        var factionIdResult = FactionId.Create(value: command.FactionId);

        if (factionIdResult.IsFailure)
        {
            return Result.Failure(errors: factionIdResult.Errors);
        }

        var faction = await factionsRepository.GetByIdAsync(
            id: factionIdResult.Value,
            cancellationToken: cancellationToken);

        if (faction is null)
        {
            return Result.Failure(
                error: Error.NotFound(message: "Faction was not found."));
        }

        var nameResult = UnitName.Create(value: command.Name);
        var descriptionResult = UnitDescription.Create(value: command.Description);
        var statsResult = command.ToCombatStats();
        var moraleResult = UnitMorale.Create(value: command.Morale);
        var luckResult = UnitLuck.Create(value: command.Luck);
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

        unit.Update(
            name: nameResult.Value,
            description: descriptionResult.Value,
            stats: statsResult.Value,
            morale: moraleResult.Value,
            luck: luckResult.Value,
            factionId: factionIdResult.Value);

        await unitsRepository.UpdateAsync(
            aggregateRoot: unit,
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}
