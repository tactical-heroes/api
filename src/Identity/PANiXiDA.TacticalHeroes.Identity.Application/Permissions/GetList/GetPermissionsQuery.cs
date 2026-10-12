namespace PANiXiDA.TacticalHeroes.Identity.Application.Permissions.GetList;

public sealed record GetPermissionsQuery()
    : IQuery<Result<IReadOnlyCollection<PermissionReadModel>>>;
