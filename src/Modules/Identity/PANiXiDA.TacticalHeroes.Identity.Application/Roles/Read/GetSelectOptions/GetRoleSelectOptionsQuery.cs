namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.Read.GetSelectOptions;

public sealed record GetRoleSelectOptionsQuery()
    : IQuery<Result<IReadOnlyList<RoleSelectOptionReadModel>>>;
