using PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetDetails;
using PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetList;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;

public interface IRolesReadRepository
{
    Task<Result<PaginationResult<RoleListItemReadModel>>> GetPagedAsync(
        PaginationParameters pagination,
        CancellationToken cancellationToken);

    Task<Result<RoleDetailsReadModel>> GetDetailsByIdAsync(
        Guid id,
        CancellationToken cancellationToken);
}
