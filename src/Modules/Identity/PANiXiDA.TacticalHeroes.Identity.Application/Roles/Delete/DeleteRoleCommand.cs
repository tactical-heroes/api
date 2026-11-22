namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.Delete;

public sealed record DeleteRoleCommand(Guid Id) : ICommand<Result>;
