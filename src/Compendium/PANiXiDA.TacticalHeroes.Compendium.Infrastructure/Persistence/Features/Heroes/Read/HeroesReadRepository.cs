using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Abstractions;
using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetDetails;
using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetList;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Heroes.Read.DbModels;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Heroes.Read.Mappers;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Features.Heroes.Read;

public sealed class HeroesReadRepository(CompendiumReadDbContext dbContext)
    : EfReadRepository<CompendiumReadDbContext, Guid, HeroReadDbModel>(dbContext),
    IHeroesReadRepository
{
    private static readonly SortParameters Sort = new(
        Field: nameof(HeroReadDbModel.Name),
        Order: SortOrder.Ascending);

    public Task<PaginationResult<HeroListItemReadModel>> GetPagedAsync(
        PaginationParameters pagination,
        CancellationToken cancellationToken)
    {
        return GetPagedResultAsync<HeroListItemReadModel, HeroListItemReadModelMapper>(
            query: Query,
            paginationParameters: pagination,
            sortParameters: Sort,
            cancellationToken: cancellationToken);
    }

    public Task<HeroDetailsReadModel?> GetDetailsByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return GetByIdAsync<HeroDetailsReadModel, HeroDetailsReadModelMapper>(
            id: id,
            cancellationToken: cancellationToken);
    }
}
