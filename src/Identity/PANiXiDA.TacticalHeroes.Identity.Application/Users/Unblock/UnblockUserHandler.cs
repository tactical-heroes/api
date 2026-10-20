using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Unblock;

public sealed class UnblockUserHandler(IUsersRepository usersRepository)
    : ICommandHandler<UnblockUserCommand, Result>, IRequireAuthorization
{
    public static IReadOnlyCollection<string> AllPermissions { get; } = [Permissions.IdentityUsers.Update.Code];

    public Task<Result> HandleAsync(
        UnblockUserCommand command,
        CancellationToken cancellationToken)
    {
        return usersRepository.UnblockAsync(command.Id, cancellationToken);
    }
}
