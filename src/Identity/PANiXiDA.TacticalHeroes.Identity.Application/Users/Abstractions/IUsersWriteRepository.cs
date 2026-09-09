using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Enumerations;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;

public interface IUsersWriteRepository
{
    Task<Result<Guid>> AddAsync(
        User user,
        UserName userName,
        string password,
        UserStatus status,
        CancellationToken cancellationToken);

    Task<Result> UpdateAsync(
        User user,
        UserName userName,
        UserStatus status,
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
