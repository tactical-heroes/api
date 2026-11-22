namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetSelectOptions;

public sealed record GetRoleSelectOptionsQuery()
    : IQuery<Result<IReadOnlyList<RoleSelectOptionReadModel>>>;
