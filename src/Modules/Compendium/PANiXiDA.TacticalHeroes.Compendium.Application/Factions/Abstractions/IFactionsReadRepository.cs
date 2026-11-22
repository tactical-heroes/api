using PANiXiDA.Core.Application.Querying.Limiting;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Common.Filters;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Read.GetDetails;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Read.GetList;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Read.GetSelectOptions;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Abstractions;

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
