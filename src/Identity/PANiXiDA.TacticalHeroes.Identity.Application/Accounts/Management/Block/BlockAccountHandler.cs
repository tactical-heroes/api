using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.Block;

public sealed class BlockAccountHandler(IAccountsWriteRepository accountsRepository)
    : ICommandHandler<BlockAccountCommand, Result>
{
    public Task<Result> HandleAsync(
        BlockAccountCommand command,
        CancellationToken cancellationToken)
    {
        return accountsRepository.BlockAsync(command.Id, cancellationToken);
    }
}
