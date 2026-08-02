using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units.Abstractions;

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

        var updateResult = unit.Update(
            name: command.Name,
            description: command.Description,
            attack: command.Attack,
            defense: command.Defense,
            health: command.Health,
            minimumDamage: command.MinimumDamage,
            maximumDamage: command.MaximumDamage,
            initiative: command.Initiative,
            speed: command.Speed,
            shots: command.Shots,
            rangedAttackRange: command.RangedAttackRange,
            morale: command.Morale,
            luck: command.Luck,
            factionId: command.FactionId);

        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        await unitsRepository.UpdateAsync(
            aggregateRoot: unit,
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}
