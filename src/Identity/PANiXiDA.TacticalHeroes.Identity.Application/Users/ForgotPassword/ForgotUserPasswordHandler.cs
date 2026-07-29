using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.ForgotPassword;

public sealed class ForgotUserPasswordHandler(IUserCredentialsService userCredentialsService)
    : ICommandHandler<ForgotUserPasswordCommand, Result>
{
    public Task<Result> HandleAsync(
        ForgotUserPasswordCommand command,
        CancellationToken cancellationToken)
    {
        return userCredentialsService.ForgotPasswordAsync(
            command.Email,
            cancellationToken);
    }
}
