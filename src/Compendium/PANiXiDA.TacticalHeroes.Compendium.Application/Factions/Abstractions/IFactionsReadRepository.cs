using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetDetails;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetList;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Abstractions;

public interface IFactionsReadRepository : IReadRepository<Guid>
{
    Task<PaginationResult<FactionListItemReadModel>> GetPagedAsync(
        PaginationParameters pagination,
        CancellationToken cancellationToken);

    Task<FactionDetailsReadModel?> GetDetailsByIdAsync(
        Guid id,
        CancellationToken cancellationToken);
}
