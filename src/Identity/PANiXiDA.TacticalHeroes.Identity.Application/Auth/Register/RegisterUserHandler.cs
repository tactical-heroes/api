using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.Register;

public sealed class RegisterUserHandler(IUserCredentialsService userCredentialsService)
    : ICommandHandler<RegisterUserCommand, Result<Guid>>
{
    public Task<Result<Guid>> HandleAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        return userCredentialsService.RegisterAsync(
            command.Email,
            command.UserName,
            command.Password,
            cancellationToken);
    }
}
