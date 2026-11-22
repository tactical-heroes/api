using PANiXiDA.TacticalHeroes.Modules.Identity.Application.Roles.GetDetails;
using PANiXiDA.TacticalHeroes.Modules.Identity.Application.Roles.GetList;
using PANiXiDA.TacticalHeroes.Modules.Identity.Application.Roles.GetSelectOptions;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.Roles.Abstractions;

public interface IRolesReadRepository : IReadRepository<Guid>
{
    Task<Guid?> GetDefaultIdAsync(CancellationToken cancellationToken);

    Task<List<RoleSelectOptionReadModel>> GetSelectOptionsAsync(CancellationToken cancellationToken);

    Task<PaginationResult<RoleListItemReadModel>> GetPageAsync(
        PaginationParameters paginationParameters,
        SortingParameters sortingParameters,
        CancellationToken cancellationToken);

    Task<RoleDetailsReadModel?> GetDetailsByIdAsync(
        Guid id,
        CancellationToken cancellationToken);
}
