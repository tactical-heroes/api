using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Confirm;

public sealed class ConfirmAccountHandler(IAccountCredentialsService accountCredentialsService)
    : ICommandHandler<ConfirmAccountCommand, Result>
{
    public Task<Result> HandleAsync(
        ConfirmAccountCommand command,
        CancellationToken cancellationToken)
    {
        return accountCredentialsService.ConfirmAsync(
            command.AccountId,
            command.EmailConfirmationToken,
            cancellationToken);
    }
}
