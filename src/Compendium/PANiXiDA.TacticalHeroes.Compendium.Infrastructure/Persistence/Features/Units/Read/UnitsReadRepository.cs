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
    private static readonly SortParameters Sort = new(
        Field: nameof(UnitReadDbModel.Name),
        Order: SortOrder.Ascending);

    public Task<PaginationResult<UnitListItemReadModel>> GetPagedAsync(
        PaginationParameters pagination,
        CancellationToken cancellationToken)
    {
        return GetPagedResultAsync<UnitListItemReadModel, UnitListItemReadModelMapper>(
            query: Query,
            paginationParameters: pagination,
            sortParameters: Sort,
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
