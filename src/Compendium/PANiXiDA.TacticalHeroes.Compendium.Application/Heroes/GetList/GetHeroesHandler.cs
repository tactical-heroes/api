using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;
using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetList;

public sealed class GetHeroesHandler(IHeroesReadRepository heroesReadRepository)
    : IQueryHandler<GetHeroesQuery, Result<PaginationResult<HeroListItemReadModel>>>, IRequireAuthorization
{
    public static IReadOnlyCollection<string> AllPermissions { get; } = [Permissions.Heroes.Read.Code];

    public async Task<Result<PaginationResult<HeroListItemReadModel>>> HandleAsync(
        GetHeroesQuery query,
        CancellationToken cancellationToken)
    {
        var heroes = await heroesReadRepository.GetPageAsync(
            paginationParameters: query.PaginationParameters,
            sortingParameters: query.SortingParameters,
            cancellationToken: cancellationToken);

        return Result.Success(value: heroes);
    }
}
