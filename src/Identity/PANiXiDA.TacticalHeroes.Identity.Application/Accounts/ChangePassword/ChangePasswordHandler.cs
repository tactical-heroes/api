using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.ChangePassword;

public sealed class ChangePasswordHandler(IAccountCredentialsService accountCredentialsService)
    : ICommandHandler<ChangePasswordCommand, Result>
{
    public Task<Result> HandleAsync(
        ChangePasswordCommand command,
        CancellationToken cancellationToken)
    {
        return accountCredentialsService.ChangePasswordAsync(
            command.AccountId,
            command.CurrentPassword,
            command.NewPassword,
            cancellationToken);
    }
}
