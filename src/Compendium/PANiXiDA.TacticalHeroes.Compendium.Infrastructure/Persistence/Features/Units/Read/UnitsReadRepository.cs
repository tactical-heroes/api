using PANiXiDA.TacticalHeroes.Compendium.Application.Units.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetDetails;
using PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetList;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Units.Read.DbModels;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Units.Read.Mappers;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Units.Read;

public sealed class UnitsReadRepository(CompendiumReadDbContext dbContext)
    : EfReadRepository<CompendiumReadDbContext, Guid, UnitReadDbModel>(dbContext),
    IUnitsReadRepository
{
    public Task<PaginationResult<UnitListItemReadModel>> GetPageAsync(
        PaginationParameters pagination,
        SortingParameters sorting,
        CancellationToken cancellationToken)
    {
        return GetPagedResultAsync<UnitListItemReadModel, UnitListItemReadModelMapper, UnitListItemReadModelSorting>(
            query: Query,
            paginationParameters: pagination,
            sortingParameters: sorting,
            cancellationToken: cancellationToken);
    }

    public Task<UnitDetailsReadModel?> GetDetailsByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return GetByIdAsync<UnitDetailsReadModel, UnitDetailsReadModelMapper>(
            id: id,
            cancellationToken: cancellationToken);
    }
}
