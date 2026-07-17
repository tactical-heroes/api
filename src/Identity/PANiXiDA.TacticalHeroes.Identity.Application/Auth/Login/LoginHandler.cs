using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.Login;

public sealed class LoginHandler(IUserCredentialsService userCredentialsService)
    : ICommandHandler<LoginCommand, Result<AuthenticatedUserReadModel>>
{
    public Task<Result<AuthenticatedUserReadModel>> HandleAsync(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        return userCredentialsService.LoginAsync(
            command.Email,
            command.Password,
            cancellationToken);
    }
}
