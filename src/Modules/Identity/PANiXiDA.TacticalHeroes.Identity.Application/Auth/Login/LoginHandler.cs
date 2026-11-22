using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.Login;

public sealed class LoginHandler(IUserCredentialsService userCredentialsService)
    : ICommandHandler<LoginCommand, Result<LoginSecurityProof>>
{
    public Task<Result<LoginSecurityProof>> HandleAsync(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        return userCredentialsService.LoginAsync(
            email: command.Email,
            password: command.Password,
            cancellationToken: cancellationToken);
    }
}
