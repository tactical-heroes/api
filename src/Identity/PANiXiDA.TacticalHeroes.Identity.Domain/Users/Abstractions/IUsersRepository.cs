namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;

public interface IUsersRepository
{
    Task<User?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken);

    Task<Result> AddAsync(
        User aggregateRoot,
        string password,
        CancellationToken cancellationToken);

    Task<Result> UpdateAsync(
        User aggregateRoot,
        CancellationToken cancellationToken);

    Task<Result> DeleteAsync(
        User aggregateRoot,
        CancellationToken cancellationToken);
}
