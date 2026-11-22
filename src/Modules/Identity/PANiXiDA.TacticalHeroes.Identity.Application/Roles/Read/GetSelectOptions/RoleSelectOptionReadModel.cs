namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.Read.GetSelectOptions;

public sealed record RoleSelectOptionReadModel(Guid Id, string Name, bool IsDefault) : IReadModel;
