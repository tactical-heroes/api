namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Write.Block;

public sealed record BlockUserCommand(Guid Id) : ICommand<Result>;
