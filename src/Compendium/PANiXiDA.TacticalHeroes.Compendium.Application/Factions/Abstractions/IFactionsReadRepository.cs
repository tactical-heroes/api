using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Filters;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetDetails;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetList;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetSelectOptions;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Abstractions;

public interface IFactionsReadRepository : IReadRepository<Guid>
{
    Task<List<FactionSelectOptionReadModel>> GetSelectOptionsAsync(
        FactionsFilter filter,
        int limit,
        CancellationToken cancellationToken);

    Task<PaginationResult<FactionListItemReadModel>> GetPageAsync(
        PaginationParameters pagination,
        CancellationToken cancellationToken);

    Task<FactionDetailsReadModel?> GetDetailsByIdAsync(
        Guid id,
        CancellationToken cancellationToken);
}
