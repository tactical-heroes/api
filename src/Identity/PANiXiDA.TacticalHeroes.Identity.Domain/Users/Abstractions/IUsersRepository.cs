namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;

public interface IUsersRepository : IRepository<UserId, User>
{
    Task<User?> GetBySpecificationAsync(
        ISpecification<User> specification,
        CancellationToken cancellationToken);
}
