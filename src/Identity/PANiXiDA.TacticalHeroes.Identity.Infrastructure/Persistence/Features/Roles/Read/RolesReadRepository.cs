using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetDetails;
using PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetList;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read.Mappers;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read;

public sealed class RolesReadRepository(IdentityReadDbContext dbContext) :
    EfReadRepository<IdentityReadDbContext, Guid, RoleReadDbModel>(dbContext),
    IRolesReadRepository
{
    public Task<PaginationResult<RoleListItemReadModel>> GetPageAsync(
        PaginationParameters pagination,
        SortingParameters sorting,
        CancellationToken cancellationToken)
    {
        return GetPagedResultAsync<RoleListItemReadModel, RoleListItemReadModelMapper, RoleListItemReadModelSorting>(
            query: Query,
            paginationParameters: pagination,
            sortingParameters: sorting,
            cancellationToken: cancellationToken);
    }

    public Task<RoleDetailsReadModel?> GetDetailsByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return GetByIdAsync<RoleDetailsReadModel, RoleDetailsReadModelMapper>(
            id: id,
            cancellationToken: cancellationToken);
    }
}
