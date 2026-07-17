using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.Login;

public sealed class LoginHandler(IAccountCredentialsService accountCredentialsService)
    : ICommandHandler<LoginCommand, Result<AuthenticatedAccountReadModel>>
{
    public Task<Result<AuthenticatedAccountReadModel>> HandleAsync(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        return accountCredentialsService.LoginAsync(
            command.Email,
            command.Password,
            cancellationToken);
    }
}
