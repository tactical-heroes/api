using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.Create;

public sealed class CreateAccountHandler(IAccountsWriteRepository accountsRepository)
    : ICommandHandler<CreateAccountCommand, Result<Guid>>
{
    public Task<Result<Guid>> HandleAsync(
        CreateAccountCommand command,
        CancellationToken cancellationToken)
    {
        return accountsRepository.AddAsync(
            command.Email,
            command.UserName,
            command.Password,
            command.IsConfirmed,
            command.Claims,
            command.Status,
            cancellationToken);
    }
}
