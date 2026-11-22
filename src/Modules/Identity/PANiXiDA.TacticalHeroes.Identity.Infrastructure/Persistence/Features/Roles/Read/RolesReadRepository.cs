using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetDetails;
using PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetList;
using PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetSelectOptions;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read.DbModels;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read.GetDetails;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read.GetList;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read.GetSelectOptions;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Read;

public sealed class RolesReadRepository(IdentityReadDbContext dbContext) :
    EfReadRepository<IdentityReadDbContext, Guid, RoleReadDbModel>(dbContext),
    IRolesReadRepository
{
    public Task<List<RoleSelectOptionReadModel>> GetSelectOptionsAsync(CancellationToken cancellationToken)
    {
        var options = RoleSelectOptionReadModelMapper.ProjectTo(Query);

        return RoleSelectOptionReadModelSorting.ApplySorting(options, SortingParameters.None)
            .ToListAsync(cancellationToken);
    }

    public Task<Guid?> GetDefaultIdAsync(CancellationToken cancellationToken)
    {
        return Query
            .Where(role => role.IsDefault)
            .Select(role => (Guid?)role.Id)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<PaginationResult<RoleListItemReadModel>> GetPageAsync(
        PaginationParameters paginationParameters,
        SortingParameters sortingParameters,
        CancellationToken cancellationToken)
    {
        return GetPagedResultAsync<RoleListItemReadModel, RoleListItemReadModelMapper, RoleListItemReadModelSorting>(
            query: Query,
            paginationParameters: paginationParameters,
            sortingParameters: sortingParameters,
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
