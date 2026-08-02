using PANiXiDA.TacticalHeroes.Compendium.Domain.Units;
using PANiXiDA.TacticalHeroes.Compendium.Domain.Units.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Units.Delete;

public sealed class DeleteUnitHandler(IUnitsRepository unitsRepository)
    : ICommandHandler<DeleteUnitCommand, Result>
{
    public async Task<Result> HandleAsync(
        DeleteUnitCommand command,
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

        await unitsRepository.DeleteAsync(
            aggregateRoot: unit,
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}
