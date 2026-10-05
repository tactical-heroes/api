using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Unblock;

public sealed class UnblockUserHandler(IUsersRepository usersRepository)
    : ICommandHandler<UnblockUserCommand, Result>
{
    public Task<Result> HandleAsync(
        UnblockUserCommand command,
        CancellationToken cancellationToken)
    {
        return usersRepository.UnblockAsync(id: command.Id, cancellationToken: cancellationToken);
    }
}
