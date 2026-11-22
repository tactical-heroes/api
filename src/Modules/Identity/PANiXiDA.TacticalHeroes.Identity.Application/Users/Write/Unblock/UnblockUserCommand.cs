namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Write.Unblock;

public sealed record UnblockUserCommand(Guid Id) : ICommand<Result>;
