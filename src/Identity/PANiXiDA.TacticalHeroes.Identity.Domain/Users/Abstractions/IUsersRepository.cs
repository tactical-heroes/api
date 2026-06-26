namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;

public interface IUsersRepository : IRepository<UserId, User>
{
    Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken);
}
