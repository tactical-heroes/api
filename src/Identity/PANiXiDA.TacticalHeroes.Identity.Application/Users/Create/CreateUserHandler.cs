using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Create;

public sealed class CreateUserHandler(IUsersWriteRepository usersRepository)
    : ICommandHandler<CreateUserCommand, Result<Guid>>
{
    public Task<Result<Guid>> HandleAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        return usersRepository.AddAsync(
            command.Email,
            command.UserName,
            command.Password,
            command.IsConfirmed,
            command.Claims,
            command.Status,
            cancellationToken);
    }
}
