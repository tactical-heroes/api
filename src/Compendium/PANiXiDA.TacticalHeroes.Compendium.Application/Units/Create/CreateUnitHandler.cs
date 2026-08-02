using PANiXiDA.TacticalHeroes.Compendium.Domain.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units.Abstractions;

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
        var unitResult = Unit.Create(
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
