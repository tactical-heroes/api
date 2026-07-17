using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.ResendConfirmationEmail;

public sealed class ResendConfirmationEmailHandler(IAccountCredentialsService accountCredentialsService)
    : ICommandHandler<ResendConfirmationEmailCommand, Result>
{
    public Task<Result> HandleAsync(
        ResendConfirmationEmailCommand command,
        CancellationToken cancellationToken)
    {
        return accountCredentialsService.ResendConfirmationEmailAsync(
            command.Email,
            cancellationToken);
    }
}
