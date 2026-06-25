using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;

public interface IUsersRepository : IRepository<UserId, User>
{
    Task<User?> GetByEmailAsync(
        Email email,
        CancellationToken cancellationToken);
}
