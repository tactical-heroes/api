using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Block;

public sealed class BlockUserHandler(IUsersRepository usersRepository)
    : ICommandHandler<BlockUserCommand, Result>, IRequireAuthorization, IRequireTransaction
{
    public static IReadOnlyCollection<string> AllPermissions { get; } = [Permissions.Identity.Users.Update.Code];

    public Task<Result> HandleAsync(
        BlockUserCommand command,
        CancellationToken cancellationToken)
    {
        return usersRepository.BlockAsync(command.Id, cancellationToken);
    }
}
