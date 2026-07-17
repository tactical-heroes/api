using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.Delete;

public sealed class DeleteAccountHandler(IAccountsWriteRepository accountsRepository)
    : ICommandHandler<DeleteAccountCommand, Result>
{
    public Task<Result> HandleAsync(
        DeleteAccountCommand command,
        CancellationToken cancellationToken)
    {
        return accountsRepository.DeleteAsync(command.Id, cancellationToken);
    }
}
