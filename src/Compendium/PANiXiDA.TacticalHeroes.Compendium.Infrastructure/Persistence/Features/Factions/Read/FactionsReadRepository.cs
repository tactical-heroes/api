using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetDetails;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetList;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Factions.Read.DbModels;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Factions.Read.Mappers;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Factions.Read;

public sealed class FactionsReadRepository(CompendiumReadDbContext dbContext)
    : EfReadRepository<CompendiumReadDbContext, Guid, FactionReadDbModel>(dbContext),
    IFactionsReadRepository
{
    private static readonly SortParameters Sort = new(
        Field: nameof(FactionReadDbModel.Name),
        Order: SortOrder.Ascending);

    public Task<PaginationResult<FactionListItemReadModel>> GetPageAsync(
        PaginationParameters pagination,
        CancellationToken cancellationToken)
    {
        return GetPagedResultAsync<FactionListItemReadModel, FactionListItemReadModelMapper>(
            query: Query,
            paginationParameters: pagination,
            sortParameters: Sort,
            cancellationToken: cancellationToken);
    }

    public Task<FactionDetailsReadModel?> GetDetailsByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return GetByIdAsync<FactionDetailsReadModel, FactionDetailsReadModelMapper>(
            id: id,
            cancellationToken: cancellationToken);
    }
}
