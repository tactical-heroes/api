namespace PANiXiDA.TacticalHeroes.Identity.Application.Roles.Read.GetDetails;

public sealed record GetRoleDetailsQuery(Guid Id)
    : IQuery<Result<RoleDetailsReadModel>>;
