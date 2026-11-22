using PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Units.Abstractions;
using PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Units.GetDetails;
using PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Units.GetList;
using PANiXiDA.TacticalHeroes.Modules.Compendium.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Modules.Compendium.Infrastructure.Persistence.Features.Units.Read.DbModels;
using PANiXiDA.TacticalHeroes.Modules.Compendium.Infrastructure.Persistence.Features.Units.Read.GetDetails;
using PANiXiDA.TacticalHeroes.Modules.Compendium.Infrastructure.Persistence.Features.Units.Read.GetList;

namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Infrastructure.Persistence.Features.Units.Read;

public sealed class UnitsReadRepository(CompendiumReadDbContext dbContext)
    :
        EfReadRepository<CompendiumReadDbContext, Guid, UnitReadDbModel>(dbContext),
        IUnitsReadRepository
{
    public Task<PaginationResult<UnitListItemReadModel>> GetPageAsync(
        PaginationParameters paginationParameters,
        SortingParameters sortingParameters,
        CancellationToken cancellationToken)
    {
        return GetPagedResultAsync<UnitListItemReadModel, UnitListItemReadModelMapper, UnitListItemReadModelSorting>(
            query: Query,
            paginationParameters: paginationParameters,
            sortingParameters: sortingParameters,
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
