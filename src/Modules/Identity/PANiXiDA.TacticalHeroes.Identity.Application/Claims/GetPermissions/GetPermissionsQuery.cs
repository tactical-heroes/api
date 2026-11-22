namespace PANiXiDA.TacticalHeroes.Identity.Application.Claims.GetPermissions;

public sealed record GetPermissionsQuery()
    : IQuery<Result<IReadOnlyCollection<PermissionReadModel>>>;
