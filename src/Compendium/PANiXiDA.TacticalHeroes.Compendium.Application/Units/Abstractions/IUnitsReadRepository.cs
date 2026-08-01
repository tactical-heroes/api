using PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetDetails;
using PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetList;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Units.Abstractions;

public interface IUnitsReadRepository : IReadRepository<Guid>
{
    Task<PaginationResult<UnitListItemReadModel>> GetPagedAsync(
        PaginationParameters pagination,
        CancellationToken cancellationToken);

    Task<UnitDetailsReadModel?> GetDetailsByIdAsync(
        Guid id,
        CancellationToken cancellationToken);
}
