namespace PANiXiDA.TacticalHeroes.Common.Authentication;

public static partial class Permissions
{
    public static IReadOnlyList<ResourceDefinition> Resources { get; } = Array.AsReadOnly<ResourceDefinition>(
    [
        IdentityUsers.Resource,
        IdentityRoles.Resource,
        Factions.Resource,
        Heroes.Resource,
        Units.Resource
    ]);

    public static IReadOnlyList<PermissionDefinition> All { get; } = Array.AsReadOnly<PermissionDefinition>(
    [
        IdentityUsers.Create,
        IdentityUsers.Read,
        IdentityUsers.Update,
        IdentityUsers.Delete,
        IdentityRoles.Create,
        IdentityRoles.Read,
        IdentityRoles.Update,
        IdentityRoles.Delete,
        Factions.Create,
        Factions.Read,
        Factions.Update,
        Factions.Delete,
        Heroes.Create,
        Heroes.Read,
        Heroes.Update,
        Heroes.Delete,
        Units.Create,
        Units.Read,
        Units.Update,
        Units.Delete
    ]);
}
