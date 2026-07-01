namespace PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Abstractions;

public interface IRolesRepository
{
    Task<Role?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<Result> AddAsync(
        Role aggregateRoot,
        CancellationToken cancellationToken);

    Task<Result> UpdateAsync(
        Role aggregateRoot,
        CancellationToken cancellationToken);

    Task<Result> DeleteAsync(
        Role aggregateRoot,
        CancellationToken cancellationToken);
}
