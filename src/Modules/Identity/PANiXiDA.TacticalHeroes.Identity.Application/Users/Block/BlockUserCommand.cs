namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Block;

public sealed record BlockUserCommand(Guid Id) : ICommand<Result>;
