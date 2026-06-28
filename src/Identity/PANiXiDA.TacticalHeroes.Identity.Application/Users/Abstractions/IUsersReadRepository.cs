using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetAuthenticated;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;

public interface IUsersReadRepository : IReadRepository<Guid>
{
    Task<AuthenticatedUserReadModel?> GetAuthenticatedUserByIdAsync(
        Guid userId,
        CancellationToken cancellationToken);
}
