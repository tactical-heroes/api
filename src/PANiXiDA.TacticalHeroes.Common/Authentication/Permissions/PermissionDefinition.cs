namespace PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;

public sealed record PermissionDefinition(
    string Code,
    string DisplayName,
    string SectionCode,
    string ResourceCode,
    string ResourceDisplayName)
{
    internal static Func<string, string, PermissionDefinition> ForResource(
        string sectionCode,
        string resourceCode,
        string resourceDisplayName) => (action, displayName) => new(
            Code: $"{sectionCode}.{resourceCode}.{action}",
            DisplayName: displayName,
            SectionCode: sectionCode,
            ResourceCode: resourceCode,
            ResourceDisplayName: resourceDisplayName);
}
