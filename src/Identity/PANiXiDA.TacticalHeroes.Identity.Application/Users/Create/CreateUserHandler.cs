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
            email: command.Email,
            userName: command.UserName,
            password: command.Password,
            isConfirmed: command.IsConfirmed,
            claims: command.Claims,
            status: command.Status,
            cancellationToken: cancellationToken);
    }
}
