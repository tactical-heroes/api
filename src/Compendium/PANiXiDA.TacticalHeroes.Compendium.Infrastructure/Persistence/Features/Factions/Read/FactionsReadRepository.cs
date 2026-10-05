using Microsoft.EntityFrameworkCore;

using PANiXiDA.Core.Application.Querying.Limiting;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Common.Filters;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetDetails;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetList;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetSelectOptions;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Factions.Read.DbModels;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Factions.Read.GetDetails;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Factions.Read.GetList;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Factions.Read.GetSelectOptions;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Factions.Read;

public sealed class FactionsReadRepository(CompendiumReadDbContext dbContext)
    : EfReadRepository<CompendiumReadDbContext, Guid, FactionReadDbModel>(dbContext: dbContext),
    IFactionsReadRepository
{
    public Task<List<FactionSelectOptionReadModel>> GetSelectOptionsAsync(
        FactionsFilter filter,
        LimitParameters limit,
        CancellationToken cancellationToken)
    {
        var query = ApplyFilter(query: Query, filter: filter);
        var options = FactionSelectOptionReadModelMapper.ProjectTo(query: query);

        return FactionSelectOptionReadModelSorting.ApplySorting(query: options, sortingParameters: SortingParameters.None)
            .Take(count: limit.Limit)
            .ToListAsync(cancellationToken: cancellationToken);
    }

    public Task<PaginationResult<FactionListItemReadModel>> GetPageAsync(
        PaginationParameters paginationParameters,
        SortingParameters sortingParameters,
        CancellationToken cancellationToken)
    {
        return GetPagedResultAsync<FactionListItemReadModel, FactionListItemReadModelMapper, FactionListItemReadModelSorting>(
            query: Query,
            paginationParameters: paginationParameters,
            sortingParameters: sortingParameters,
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
            query = query.Where(predicate: faction =>
                EF.Functions.ILike(
                    matchExpression: faction.Name,
                    pattern: $"%{filter.Search.Trim()}%"));
        }

        return query;
    }
}
