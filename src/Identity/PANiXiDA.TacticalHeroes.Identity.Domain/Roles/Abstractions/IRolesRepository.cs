namespace PANiXiDA.TacticalHeroes.Identity.Domain.Roles.Abstractions;

public interface IRolesRepository
{
    Task<Result<Role?>> GetDefaultAsync(CancellationToken cancellationToken);

    Task<Result<Guid>> AddAsync(
        Role role,
        CancellationToken cancellationToken);

    Task<Result> UpdateAsync(
        Role role,
        CancellationToken cancellationToken);

    Task<Result> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken);
}
