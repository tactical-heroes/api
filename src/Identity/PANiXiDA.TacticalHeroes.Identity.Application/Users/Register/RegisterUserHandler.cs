using PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Register;

public sealed class RegisterUserHandler(
    IUserCredentialsService userCredentialsService)
    : ICommandHandler<RegisterUserCommand, Result<Guid>>
{
    public Task<Result<Guid>> HandleAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        return userCredentialsService.RegisterAsync(
            command.Email,
            command.Password,
            cancellationToken);
    }
}
