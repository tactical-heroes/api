namespace PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;

public static partial class Permissions
{
    public static partial class FileManager
    {
        public static class PersonalFiles
        {
            public const string ResourceCode = "personal-files";
            public const string ResourceName = "Персональные файлы";

            public static readonly PermissionDefinition CreateOwn =
                DefinePermission("create.own", "Создание своих персональных файлов");

            public static readonly PermissionDefinition CreateAll =
                DefinePermission("create.all", "Создание всех персональных файлов");

            public static readonly PermissionDefinition ReadOwn =
                DefinePermission("read.own", "Просмотр своих персональных файлов");

            public static readonly PermissionDefinition ReadAll =
                DefinePermission("read.all", "Просмотр всех персональных файлов");

            public static readonly PermissionDefinition UpdateOwn =
                DefinePermission("update.own", "Изменение своих персональных файлов");

            public static readonly PermissionDefinition UpdateAll =
                DefinePermission("update.all", "Изменение всех персональных файлов");

            public static readonly PermissionDefinition DeleteOwn =
                DefinePermission("delete.own", "Удаление своих персональных файлов");

            public static readonly PermissionDefinition DeleteAll =
                DefinePermission("delete.all", "Удаление всех персональных файлов");

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
