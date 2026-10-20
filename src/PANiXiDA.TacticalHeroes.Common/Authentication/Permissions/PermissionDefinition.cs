namespace PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;

public sealed record PermissionDefinition(
    string Code,
    string DisplayName,
    string ModuleCode,
    string ModuleDisplayName,
    string ResourceCode,
    string ResourceDisplayName)
{
    internal static PermissionDefinition Create(
        string moduleCode,
        string moduleDisplayName,
        string resourceCode,
        string resourceDisplayName,
        string action,
        string displayName) => new(
            Code: $"{moduleCode}.{resourceCode}.{action}",
            DisplayName: displayName,
            ModuleCode: moduleCode,
            ModuleDisplayName: moduleDisplayName,
            ResourceCode: resourceCode,
            ResourceDisplayName: resourceDisplayName);
}
