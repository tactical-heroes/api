using PANiXiDA.TacticalHeroes.Identity.Domain.Users.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Delete;

public sealed class DeleteUserHandler(IUsersRepository usersRepository)
    : ICommandHandler<DeleteUserCommand, Result>
{
    public Task<Result> HandleAsync(
        DeleteUserCommand command,
        CancellationToken cancellationToken)
    {
        return usersRepository.DeleteAsync(id: command.Id, cancellationToken: cancellationToken);
    }
}
