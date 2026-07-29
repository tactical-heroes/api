using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetList;

public sealed class GetFactionListHandler(IFactionsReadRepository factionsReadRepository)
    : IQueryHandler<GetFactionListQuery, Result<PaginationResult<FactionListItemReadModel>>>
{
    public async Task<Result<PaginationResult<FactionListItemReadModel>>> HandleAsync(
        GetFactionListQuery query,
        CancellationToken cancellationToken)
    {
        var factions = await factionsReadRepository.GetPagedAsync(
            pagination: query.Pagination,
            cancellationToken: cancellationToken);

        return Result.Success(value: factions);
    }
}
