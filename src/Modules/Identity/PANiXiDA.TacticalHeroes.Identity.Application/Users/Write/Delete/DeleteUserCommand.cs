namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Write.Delete;

public sealed record DeleteUserCommand(Guid Id) : ICommand<Result>;
