using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers.Abstractions;

public interface IIdentityUsersRepository : IRepository<IdentityUserId, IdentityUser>
{
    Task<IdentityUser?> GetByEmailAsync(
        Email email,
        CancellationToken cancellationToken);
}
