using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetDetails;
using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetList;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Abstractions;

public interface IHeroesReadRepository : IReadRepository<Guid>
{
    Task<PaginationResult<HeroListItemReadModel>> GetPageAsync(
        PaginationParameters pagination,
        CancellationToken cancellationToken);

    Task<HeroDetailsReadModel?> GetDetailsByIdAsync(
        Guid id,
        CancellationToken cancellationToken);
}
