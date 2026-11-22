namespace PANiXiDA.TacticalHeroes.Identity.Application.Claims.GetPermissions;

public sealed record PermissionReadModel(
    string Code,
    string DisplayName,
    string ModuleCode,
    string ModuleDisplayName,
    string ResourceCode,
    string ResourceDisplayName) : IReadModel;
