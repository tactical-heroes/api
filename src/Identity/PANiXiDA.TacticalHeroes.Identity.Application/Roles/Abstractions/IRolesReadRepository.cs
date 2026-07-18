using PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetDetails;
using PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetList;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;

public interface IRolesReadRepository : IReadRepository<Guid>
{
    Task<PaginationResult<RoleListItemReadModel>> GetPagedAsync(
        PaginationParameters pagination,
        CancellationToken cancellationToken);

    Task<RoleDetailsReadModel?> GetDetailsByIdAsync(
        Guid id,
        CancellationToken cancellationToken);
}
