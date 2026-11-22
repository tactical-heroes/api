namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.Roles.GetSelectOptions;

public sealed record GetRoleSelectOptionsQuery()
    : IQuery<Result<IReadOnlyList<RoleSelectOptionReadModel>>>;
