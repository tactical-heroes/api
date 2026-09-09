using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;

public interface IRolesWriteRepository
{
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
