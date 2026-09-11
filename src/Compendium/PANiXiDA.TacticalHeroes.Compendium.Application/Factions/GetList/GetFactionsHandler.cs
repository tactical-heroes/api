using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetList;

public sealed class GetFactionsHandler(IFactionsReadRepository factionsReadRepository)
    : IQueryHandler<GetFactionsQuery, Result<PaginationResult<FactionListItemReadModel>>>
{
    public async Task<Result<PaginationResult<FactionListItemReadModel>>> HandleAsync(
        GetFactionsQuery query,
        CancellationToken cancellationToken)
    {
        var factions = await factionsReadRepository.GetPageAsync(
            pagination: query.Pagination,
            cancellationToken: cancellationToken);

        return Result.Success(value: factions);
    }
}
