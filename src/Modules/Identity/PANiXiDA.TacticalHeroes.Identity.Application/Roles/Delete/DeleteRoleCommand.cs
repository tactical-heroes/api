namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.Roles.Delete;

public sealed record DeleteRoleCommand(Guid Id) : ICommand<Result>;
