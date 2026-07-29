using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Login;

public sealed class LoginUserHandler(IUserCredentialsService userCredentialsService)
    : ICommandHandler<LoginUserCommand, Result<AuthenticatedUserReadModel>>
{
    public Task<Result<AuthenticatedUserReadModel>> HandleAsync(
        LoginUserCommand command,
        CancellationToken cancellationToken)
    {
        return userCredentialsService.LoginAsync(
            command.Email,
            command.Password,
            cancellationToken);
    }
}
