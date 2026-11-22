using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Read.GetDetails;
using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Read.GetList;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Abstractions;

public interface IHeroesReadRepository : IReadRepository<Guid>
{
    Task<PaginationResult<HeroListItemReadModel>> GetPageAsync(
        PaginationParameters paginationParameters,
        SortingParameters sortingParameters,
        CancellationToken cancellationToken);

    Task<HeroDetailsReadModel?> GetDetailsByIdAsync(
        Guid id,
        CancellationToken cancellationToken);
}
