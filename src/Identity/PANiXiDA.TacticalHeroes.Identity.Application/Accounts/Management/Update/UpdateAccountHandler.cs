using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.Update;

public sealed class UpdateAccountHandler(IAccountsWriteRepository accountsRepository)
    : ICommandHandler<UpdateAccountCommand, Result>
{
    public Task<Result> HandleAsync(
        UpdateAccountCommand command,
        CancellationToken cancellationToken)
    {
        return accountsRepository.UpdateAsync(
            command.Id,
            command.Email,
            command.UserName,
            command.IsConfirmed,
            command.Claims,
            command.Status,
            cancellationToken);
    }
}
