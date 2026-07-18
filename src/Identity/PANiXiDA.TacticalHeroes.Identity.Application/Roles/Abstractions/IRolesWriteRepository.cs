using System.Security.Claims;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.Abstractions;

public interface IRolesWriteRepository
{
    Task<Result<Guid>> AddAsync(
        string name,
        IReadOnlyCollection<Claim> claims,
        CancellationToken cancellationToken);

    Task<Result> UpdateAsync(
        Guid id,
        string name,
        IReadOnlyCollection<Claim> claims,
        CancellationToken cancellationToken);

    Task<Result> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken);
}
