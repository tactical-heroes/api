using PANiXiDA.TacticalHeroes.Compendium.Application.Units.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetList;

public sealed class GetUnitsHandler(IUnitsReadRepository unitsReadRepository)
    : IQueryHandler<GetUnitsQuery, Result<PaginationResult<UnitListItemReadModel>>>
{
    public async Task<Result<PaginationResult<UnitListItemReadModel>>> HandleAsync(
        GetUnitsQuery query,
        CancellationToken cancellationToken)
    {
        var units = await unitsReadRepository.GetPagedAsync(
            pagination: query.Pagination,
            cancellationToken: cancellationToken);

        return Result.Success(value: units);
    }
}
