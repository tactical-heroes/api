using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Block;

public sealed class BlockUserHandler(IUsersRepository usersRepository)
    : ICommandHandler<BlockUserCommand, Result>
{
    public Task<Result> HandleAsync(
        BlockUserCommand command,
        CancellationToken cancellationToken)
    {
        return usersRepository.BlockAsync(command.Id, cancellationToken);
    }
}
