namespace PANiXiDA.TacticalHeroes.Identity.Application.Claims.GetPermissions;

public sealed record PermissionReadModel(
    string Code,
    string DisplayName,
    string SectionCode,
    string ResourceCode,
    string ResourceDisplayName) : IReadModel;
