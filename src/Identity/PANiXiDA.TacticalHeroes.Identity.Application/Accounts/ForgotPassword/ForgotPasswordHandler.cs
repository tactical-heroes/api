using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.ForgotPassword;

public sealed class ForgotPasswordHandler(IAccountCredentialsService accountCredentialsService)
    : ICommandHandler<ForgotPasswordCommand, Result>
{
    public Task<Result> HandleAsync(
        ForgotPasswordCommand command,
        CancellationToken cancellationToken)
    {
        return accountCredentialsService.ForgotPasswordAsync(
            command.Email,
            cancellationToken);
    }
}
