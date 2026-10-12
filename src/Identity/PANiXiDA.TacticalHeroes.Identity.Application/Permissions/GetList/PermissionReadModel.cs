namespace PANiXiDA.TacticalHeroes.Identity.Application.Permissions.GetList;

public sealed record PermissionReadModel(
    string Code,
    string DisplayName,
    string SectionCode,
    string SectionDisplayName,
    string ResourceCode,
    string ResourceDisplayName) : IReadModel;
