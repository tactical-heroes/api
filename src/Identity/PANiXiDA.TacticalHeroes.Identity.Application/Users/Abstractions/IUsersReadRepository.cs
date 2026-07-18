using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetDetails;
using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetList;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;

public interface IUsersReadRepository : IReadRepository<Guid>
{
    Task<PaginationResult<UserListItemReadModel>> GetPagedAsync(
        string? email,
        PaginationParameters pagination,
        CancellationToken cancellationToken);

    Task<UserDetailsReadModel?> GetDetailsByIdAsync(
        Guid id,
        CancellationToken cancellationToken);
}
