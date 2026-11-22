namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.Roles.GetDetails;

public sealed record GetRoleDetailsQuery(Guid Id)
    : IQuery<Result<RoleDetailsReadModel>>;
