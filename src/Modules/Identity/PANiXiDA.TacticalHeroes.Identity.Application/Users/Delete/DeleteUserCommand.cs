namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.Users.Delete;

public sealed record DeleteUserCommand(Guid Id) : ICommand<Result>;
