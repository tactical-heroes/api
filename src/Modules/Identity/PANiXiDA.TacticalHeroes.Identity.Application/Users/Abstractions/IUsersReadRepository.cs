using PANiXiDA.TacticalHeroes.Modules.Identity.Application.Users.Common.Filters;
using PANiXiDA.TacticalHeroes.Modules.Identity.Application.Users.GetDetails;
using PANiXiDA.TacticalHeroes.Modules.Identity.Application.Users.GetList;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.Users.Abstractions;

public interface IUsersReadRepository : IReadRepository<Guid>
{
    Task<PaginationResult<UserListItemReadModel>> GetPageAsync(
        UsersFilter filter,
        PaginationParameters paginationParameters,
        SortingParameters sortingParameters,
        CancellationToken cancellationToken);

    Task<UserDetailsReadModel?> GetDetailsByIdAsync(
        Guid id,
        CancellationToken cancellationToken);
}
