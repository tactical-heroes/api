namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.Unblock;

public sealed record UnblockAccountCommand(Guid Id) : ICommand<Result>;
