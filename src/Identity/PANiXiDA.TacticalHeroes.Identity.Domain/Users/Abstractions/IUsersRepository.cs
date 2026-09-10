namespace PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;

public interface IUsersRepository
{
    Task<Result<Guid>> AddAsync(
        User user,
        string password,
        CancellationToken cancellationToken);

    Task<Result> UpdateAsync(
        User user,
        CancellationToken cancellationToken);

    Task<Result> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<Result> BlockAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<Result> UnblockAsync(
        Guid id,
        CancellationToken cancellationToken);
}
