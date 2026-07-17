namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.GetDetails;

public sealed record GetRoleDetailsQuery(Guid Id)
    : IQuery<Result<RoleDetailsReadModel>>;
