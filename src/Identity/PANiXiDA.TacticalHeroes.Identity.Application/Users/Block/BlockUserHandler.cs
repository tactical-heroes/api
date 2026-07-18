using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Block;

public sealed class BlockUserHandler(IUsersWriteRepository usersRepository)
    : ICommandHandler<BlockUserCommand, Result>
{
    public Task<Result> HandleAsync(
        BlockUserCommand command,
        CancellationToken cancellationToken)
    {
        return usersRepository.BlockAsync(command.Id, cancellationToken);
    }
}
