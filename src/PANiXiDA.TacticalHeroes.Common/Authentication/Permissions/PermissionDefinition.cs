namespace PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;

public sealed record PermissionDefinition(
    string Code,
    string DisplayName,
    string SectionCode,
    string ResourceCode,
    string ResourceDisplayName)
{
    internal static PermissionDefinition Create(
        string sectionCode,
        string resourceCode,
        string resourceDisplayName,
        string action,
        string displayName) => new(
            Code: $"{sectionCode}.{resourceCode}.{action}",
            DisplayName: displayName,
            SectionCode: sectionCode,
            ResourceCode: resourceCode,
            ResourceDisplayName: resourceDisplayName);
}
