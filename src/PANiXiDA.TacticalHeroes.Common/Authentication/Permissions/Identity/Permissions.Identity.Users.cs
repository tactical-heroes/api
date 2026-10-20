namespace PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;

public static partial class Permissions
{
    public static partial class Identity
    {
        public static class Users
        {
            public const string ResourceCode = "users";
            public const string ResourceName = "Пользователи";

            public static readonly PermissionDefinition Create =
                DefineUserPermission("create", "Создание пользователей");

            public static readonly PermissionDefinition Read =
                DefineUserPermission("read", "Просмотр пользователей");

            public static readonly PermissionDefinition Update =
                DefineUserPermission("update", "Изменение пользователей");

            public static readonly PermissionDefinition Delete =
                DefineUserPermission("delete", "Удаление пользователей");

            private static PermissionDefinition DefineUserPermission(string action, string displayName) =>
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
