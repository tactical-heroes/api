using System.Security.Claims;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;

public interface IUsersWriteRepository
{
    Task<Result<Guid>> AddAsync(
        string email,
        string userName,
        string password,
        bool isConfirmed,
        IReadOnlyCollection<Claim> claims,
        string status,
        CancellationToken cancellationToken);

    Task<Result> UpdateAsync(
        Guid id,
        string email,
        string userName,
        bool isConfirmed,
        IReadOnlyCollection<Claim> claims,
        string status,
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
