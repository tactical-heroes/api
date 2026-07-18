namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Delete;

public sealed record DeleteUserCommand(Guid Id) : ICommand<Result>;
