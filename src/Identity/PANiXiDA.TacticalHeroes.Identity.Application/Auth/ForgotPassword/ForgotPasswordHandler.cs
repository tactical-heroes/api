using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.ForgotPassword;

public sealed class ForgotPasswordHandler(IUserCredentialsService userCredentialsService)
    : ICommandHandler<ForgotPasswordCommand, Result>
{
    public Task<Result> HandleAsync(
        ForgotPasswordCommand command,
        CancellationToken cancellationToken)
    {
        return userCredentialsService.ForgotPasswordAsync(
            command.Email,
            cancellationToken);
    }
}
