using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Delete;

public sealed class DeleteUserHandler(IUsersWriteRepository usersRepository)
    : ICommandHandler<DeleteUserCommand, Result>
{
    public Task<Result> HandleAsync(
        DeleteUserCommand command,
        CancellationToken cancellationToken)
    {
        return usersRepository.DeleteAsync(command.Id, cancellationToken);
    }
}
