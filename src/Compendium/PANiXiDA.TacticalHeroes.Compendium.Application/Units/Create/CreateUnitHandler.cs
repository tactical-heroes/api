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
        var unitResult = Unit.Create(new UnitAttributes
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

        if (unitResult.IsFailure)
        {
            return Result.Failure<Guid>(errors: unitResult.Errors);
        }

        var faction = await factionsRepository.GetByIdAsync(
            id: unitResult.Value.FactionId,
            cancellationToken: cancellationToken);

        if (faction is null)
        {
            return Result.Failure<Guid>(
                error: Error.NotFound(message: "Faction was not found."));
        }

        await unitsRepository.AddAsync(
            aggregateRoot: unitResult.Value,
            cancellationToken: cancellationToken);

        return Result.Success(value: unitResult.Value.Id.Value);
    }
}
