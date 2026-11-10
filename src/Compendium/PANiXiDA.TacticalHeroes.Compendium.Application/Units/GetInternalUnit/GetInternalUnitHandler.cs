using PANiXiDA.TacticalHeroes.Compendium.Application.Units.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetDetails;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetInternalUnit;

public sealed class GetInternalUnitHandler(IUnitsReadRepository unitsReadRepository)
    : IQueryHandler<GetInternalUnitQuery, Result<UnitDetailsReadModel>>
{
    public async Task<Result<UnitDetailsReadModel>> HandleAsync(
        GetInternalUnitQuery query,
        CancellationToken cancellationToken)
    {
        var unit = await unitsReadRepository.GetDetailsByIdAsync(query.Id, cancellationToken);

        return unit is null
            ? Result.Failure<UnitDetailsReadModel>(Error.NotFound("Unit was not found."))
            : Result.Success(unit);
    }
}
