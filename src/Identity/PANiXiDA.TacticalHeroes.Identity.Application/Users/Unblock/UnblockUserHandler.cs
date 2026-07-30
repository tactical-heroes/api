using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Unblock;

public sealed class UnblockUserHandler(IUsersWriteRepository usersRepository)
    : ICommandHandler<UnblockUserCommand, Result>
{
    public Task<Result> HandleAsync(
        UnblockUserCommand command,
        CancellationToken cancellationToken)
    {
        return usersRepository.UnblockAsync(command.Id, cancellationToken);
    }
}
