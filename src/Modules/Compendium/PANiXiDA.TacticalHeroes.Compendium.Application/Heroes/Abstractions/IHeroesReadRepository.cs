using PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Heroes.GetDetails;
using PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Heroes.GetList;

namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Heroes.Abstractions;

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
