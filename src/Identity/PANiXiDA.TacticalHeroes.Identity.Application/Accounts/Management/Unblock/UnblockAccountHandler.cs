using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.Unblock;

public sealed class UnblockAccountHandler(IAccountsWriteRepository accountsRepository)
    : ICommandHandler<UnblockAccountCommand, Result>
{
    public Task<Result> HandleAsync(
        UnblockAccountCommand command,
        CancellationToken cancellationToken)
    {
        return accountsRepository.UnblockAsync(command.Id, cancellationToken);
    }
}
