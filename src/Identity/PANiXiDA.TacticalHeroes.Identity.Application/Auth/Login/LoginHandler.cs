using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.Login;

public sealed class LoginHandler(IUserCredentialsService userCredentialsService)
    : ICommandHandler<LoginCommand, Result<Guid>>
{
    public Task<Result<Guid>> HandleAsync(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        return userCredentialsService.LoginAsync(
            email: command.Email,
            password: command.Password,
            cancellationToken: cancellationToken);
    }
}
