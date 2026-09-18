using Microsoft.EntityFrameworkCore;

using PANiXiDA.Core.Application.Querying.Limiting;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Common.Filters;
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
    public Task<List<FactionSelectOptionReadModel>> GetSelectOptionsAsync(
        FactionsFilter filter,
        LimitParameters limit,
        CancellationToken cancellationToken)
    {
        var query = ApplyFilter(Query, filter);
        var options = FactionSelectOptionReadModelMapper.ProjectTo(query);

        return FactionSelectOptionReadModelSorting.ApplySorting(options, SortingParameters.None)
            .Take(limit.Limit)
            .ToListAsync(cancellationToken);
    }

    public Task<PaginationResult<FactionListItemReadModel>> GetPageAsync(
        PaginationParameters pagination,
        SortingParameters sorting,
        CancellationToken cancellationToken)
    {
        return GetPagedResultAsync<FactionListItemReadModel, FactionListItemReadModelMapper, FactionListItemReadModelSorting>(
            query: Query,
            paginationParameters: pagination,
            sortingParameters: sorting,
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

    private static IQueryable<FactionReadDbModel> ApplyFilter(
        IQueryable<FactionReadDbModel> query,
        FactionsFilter filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(faction =>
                EF.Functions.ILike(
                    matchExpression: faction.Name,
                    pattern: $"%{filter.Search.Trim()}%"));
        }

        return query;
    }
}
