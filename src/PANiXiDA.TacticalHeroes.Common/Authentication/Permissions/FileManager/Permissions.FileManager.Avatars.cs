namespace PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;

public static partial class Permissions
{
    public static partial class FileManager
    {
        public static class Avatars
        {
            public const string ResourceCode = "avatars";
            public const string ResourceName = "Аватары";

            public static readonly PermissionDefinition Create =
                DefinePermission("create", "Создание аватаров");

            public static readonly PermissionDefinition Read =
                DefinePermission("read", "Просмотр аватаров");

            public static readonly PermissionDefinition Update =
                DefinePermission("update", "Изменение аватаров");

            public static readonly PermissionDefinition Delete =
                DefinePermission("delete", "Удаление аватаров");

            private static PermissionDefinition DefinePermission(string action, string displayName) =>
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
