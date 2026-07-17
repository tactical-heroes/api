using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Register;

public sealed class RegisterAccountHandler(IAccountCredentialsService accountCredentialsService)
    : ICommandHandler<RegisterAccountCommand, Result<Guid>>
{
    public Task<Result<Guid>> HandleAsync(
        RegisterAccountCommand command,
        CancellationToken cancellationToken)
    {
        return accountCredentialsService.RegisterAsync(
            command.Email,
            command.UserName,
            command.Password,
            cancellationToken);
    }
}
