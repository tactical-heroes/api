namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.Users.Block;

public sealed record BlockUserCommand(Guid Id) : ICommand<Result>;
