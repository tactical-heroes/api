namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.Write.Delete;

public sealed record DeleteRoleCommand(Guid Id) : ICommand<Result>;
