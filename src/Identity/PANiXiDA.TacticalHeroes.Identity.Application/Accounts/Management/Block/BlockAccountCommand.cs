namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.Block;

public sealed record BlockAccountCommand(Guid Id) : ICommand<Result>;
