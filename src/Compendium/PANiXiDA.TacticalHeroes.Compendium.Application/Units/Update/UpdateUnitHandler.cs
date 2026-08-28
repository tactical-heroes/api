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

        var updateResult = unit.Update(new UnitAttributes
        {
            Name = command.Name,
            Description = command.Description,
            CombatStats = new UnitCombatStatsInput
            {
                Attack = command.Attack,
                Defense = command.Defense,
                Health = command.Health,
                MinimumDamage = command.MinimumDamage,
                MaximumDamage = command.MaximumDamage,
                Initiative = command.Initiative,
                Speed = command.Speed,
                Shots = command.Shots,
                RangedAttackRange = command.RangedAttackRange
            },
            Morale = command.Morale,
            Luck = command.Luck,
            FactionId = command.FactionId
        });

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
