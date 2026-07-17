namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.Delete;

public sealed record DeleteAccountCommand(Guid Id) : ICommand<Result>;
