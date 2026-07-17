using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.ResetPassword;

public sealed class ResetPasswordHandler(IAccountCredentialsService accountCredentialsService)
    : ICommandHandler<ResetPasswordCommand, Result>
{
    public Task<Result> HandleAsync(
        ResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        return accountCredentialsService.ResetPasswordAsync(
            command.AccountId,
            command.PasswordResetToken,
            command.NewPassword,
            cancellationToken);
    }
}
