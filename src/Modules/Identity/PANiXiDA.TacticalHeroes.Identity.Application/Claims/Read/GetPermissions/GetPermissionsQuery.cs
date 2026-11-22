namespace PANiXiDA.TacticalHeroes.Identity.Application.Claims.Read.GetPermissions;

public sealed record GetPermissionsQuery()
    : IQuery<Result<IReadOnlyCollection<PermissionReadModel>>>;
