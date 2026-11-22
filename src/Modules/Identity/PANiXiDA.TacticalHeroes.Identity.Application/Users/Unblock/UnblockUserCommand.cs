namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Unblock;

public sealed record UnblockUserCommand(Guid Id) : ICommand<Result>;
