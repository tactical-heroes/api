using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Update;

public sealed class UpdateUserHandler(IUsersWriteRepository usersRepository)
    : ICommandHandler<UpdateUserCommand, Result>
{
    public Task<Result> HandleAsync(
        UpdateUserCommand command,
        CancellationToken cancellationToken)
    {
        return usersRepository.UpdateAsync(
            command.Id,
            command.Email,
            command.UserName,
            command.IsConfirmed,
            command.Claims,
            command.Status,
            cancellationToken);
    }
}
