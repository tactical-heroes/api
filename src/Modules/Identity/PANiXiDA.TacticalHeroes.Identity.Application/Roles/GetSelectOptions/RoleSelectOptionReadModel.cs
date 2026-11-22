namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.Roles.GetSelectOptions;

public sealed record RoleSelectOptionReadModel(Guid Id, string Name, bool IsDefault) : IReadModel;
