using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetList;

public sealed class GetFactionsHandler(IFactionsReadRepository factionsRepository)
    : IQueryHandler<GetFactionsQuery, Result<PaginationResult<FactionListItemReadModel>>>
{
    public async Task<Result<PaginationResult<FactionListItemReadModel>>> HandleAsync(
        GetFactionsQuery query,
        CancellationToken cancellationToken)
    {
        var factions = await factionsRepository.GetPagedAsync(
            pagination: query.Pagination,
            cancellationToken: cancellationToken);

        return Result.Success(value: factions);
    }
}
