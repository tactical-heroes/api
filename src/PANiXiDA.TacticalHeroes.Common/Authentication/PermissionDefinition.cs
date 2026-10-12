namespace PANiXiDA.TacticalHeroes.Common.Authentication;

public sealed record PermissionDefinition(
    string Code,
    string DisplayName,
    string SectionCode,
    string ResourceCode);
