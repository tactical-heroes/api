using PANiXiDA.TacticalHeroes.Compendium.Application.Units.Read.GetDetails;
using PANiXiDA.TacticalHeroes.Compendium.Application.Units.Read.GetList;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Units.Abstractions;

public interface IUnitsReadRepository : IReadRepository<Guid>
{
    Task<PaginationResult<UnitListItemReadModel>> GetPageAsync(
        PaginationParameters paginationParameters,
        SortingParameters sortingParameters,
        CancellationToken cancellationToken);

    Task<UnitDetailsReadModel?> GetDetailsByIdAsync(
        Guid id,
        CancellationToken cancellationToken);
}
