namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;

public interface IUsersReadRepository : IReadRepository<Guid>
{
    Task<AuthenticatedUser?> GetAuthenticatedUserByIdAsync(
        Guid userId,
        CancellationToken cancellationToken);
}
