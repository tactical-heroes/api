using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetSelectOptions;

public sealed class GetFactionSelectOptionsHandler(IFactionsReadRepository factionsReadRepository)
    : IQueryHandler<GetFactionSelectOptionsQuery, Result<IReadOnlyList<FactionSelectOptionReadModel>>>
{
    public async Task<Result<IReadOnlyList<FactionSelectOptionReadModel>>> HandleAsync(
        GetFactionSelectOptionsQuery query,
        CancellationToken cancellationToken)
    {
        var options = await factionsReadRepository.GetSelectOptionsAsync(
            search: query.Search,
            limit: query.Limit,
            cancellationToken: cancellationToken);

        return Result.Success(value: options);
    }
}
