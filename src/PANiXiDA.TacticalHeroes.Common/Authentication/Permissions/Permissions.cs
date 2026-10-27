namespace PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;

public static partial class Permissions
{
    public static IReadOnlyList<PermissionDefinition> All { get; } = Array.AsReadOnly(
    [
        Compendium.Factions.Create,
        Compendium.Factions.Read,
        Compendium.Factions.Update,
        Compendium.Factions.Delete,
        Compendium.Heroes.Create,
        Compendium.Heroes.Read,
        Compendium.Heroes.Update,
        Compendium.Heroes.Delete,
        Compendium.Units.Create,
        Compendium.Units.Read,
        Compendium.Units.Update,
        Compendium.Units.Delete,
        FileManager.PersonalFiles.CreateOwn,
        FileManager.PersonalFiles.CreateAll,
        FileManager.PersonalFiles.ReadOwn,
        FileManager.PersonalFiles.ReadAll,
        FileManager.PersonalFiles.UpdateOwn,
        FileManager.PersonalFiles.UpdateAll,
        FileManager.PersonalFiles.DeleteOwn,
        FileManager.PersonalFiles.DeleteAll,
        FileManager.Avatars.Create,
        FileManager.Avatars.Read,
        FileManager.Avatars.Update,
        FileManager.Avatars.Delete,
        Identity.Roles.Create,
        Identity.Roles.Read,
        Identity.Roles.Update,
        Identity.Roles.Delete,
        Identity.Users.Create,
        Identity.Users.Read,
        Identity.Users.Update,
        Identity.Users.Delete
    ]);
}
