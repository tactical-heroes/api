namespace PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;

public static partial class Permissions
{
    public static partial class Compendium
    {
        public static class Heroes
        {
            public const string ResourceCode = "heroes";
            public const string ResourceName = "Герои";

            public static readonly PermissionDefinition Create =
                DefineHeroPermission("create", "Создание героев");

            public static readonly PermissionDefinition Read =
                DefineHeroPermission("read", "Просмотр героев");

            public static readonly PermissionDefinition Update =
                DefineHeroPermission("update", "Изменение героев");

            public static readonly PermissionDefinition Delete =
                DefineHeroPermission("delete", "Удаление героев");

            private static PermissionDefinition DefineHeroPermission(string action, string displayName) =>
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
