using PANiXiDA.Core.Application.Querying.Limiting;
using PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Factions.Common.Filters;
using PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Factions.GetDetails;
using PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Factions.GetList;
using PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Factions.GetSelectOptions;

namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Factions.Abstractions;

public interface IFactionsReadRepository : IReadRepository<Guid>
{
    Task<List<FactionSelectOptionReadModel>> GetSelectOptionsAsync(
        FactionsFilter filter,
        LimitParameters limit,
        CancellationToken cancellationToken);

    Task<PaginationResult<FactionListItemReadModel>> GetPageAsync(
        PaginationParameters paginationParameters,
        SortingParameters sortingParameters,
        CancellationToken cancellationToken);

    Task<FactionDetailsReadModel?> GetDetailsByIdAsync(
        Guid id,
        CancellationToken cancellationToken);
}
