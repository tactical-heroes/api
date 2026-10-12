namespace PANiXiDA.TacticalHeroes.Common.Authentication;

public sealed record ResourceDefinition(
    string Code,
    string DisplayName,
    string SectionCode)
{
    public PermissionDefinition CreatePermission(string action, string displayName) => new(
        Code: $"{SectionCode}.{Code}.{action}",
        DisplayName: displayName,
        SectionCode: SectionCode,
        ResourceCode: Code);
}
