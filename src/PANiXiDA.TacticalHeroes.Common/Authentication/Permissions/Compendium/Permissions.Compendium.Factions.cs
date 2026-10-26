namespace PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;

public static partial class Permissions
{
    public static partial class Compendium
    {
        public static class Factions
        {
            public const string ResourceCode = "factions";
            public const string ResourceName = "Фракции";

            public static readonly PermissionDefinition Create =
                DefineFactionPermission("create", "Создание фракций");

            public static readonly PermissionDefinition Read =
                DefineFactionPermission("read", "Просмотр фракций");

            public static readonly PermissionDefinition Update =
                DefineFactionPermission("update", "Изменение фракций");

            public static readonly PermissionDefinition Delete =
                DefineFactionPermission("delete", "Удаление фракций");

            private static PermissionDefinition DefineFactionPermission(string action, string displayName) =>
                PermissionDefinition.Create(
                    moduleCode: Code,
                    moduleDisplayName: DisplayName,
                    resourceCode: ResourceCode,
                    resourceDisplayName: ResourceName,
                    action: action,
                    displayName: displayName);
        }
    }
}
