using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetDetails;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetList;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetSelectOptions;
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

    public Task<List<FactionSelectOptionReadModel>> GetSelectOptionsAsync(
        string? search,
        int limit,
        CancellationToken cancellationToken)
    {
        var query = Query;
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(faction =>
                EF.Functions.ILike(
                    matchExpression: faction.Name,
                    pattern: $"%{search.Trim()}%"));
        }

        query = query.OrderBy(faction => faction.Name)
            .ThenBy(faction => faction.Id)
            .Take(limit);

        return FactionSelectOptionReadModelMapper.ProjectTo(query)
            .ToListAsync(cancellationToken);
    }

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
