namespace PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;

public static partial class Permissions
{
    public static partial class Identity
    {
        public static class Roles
        {
            public const string ResourceCode = "roles";
            public const string ResourceName = "Роли";

            public static readonly PermissionDefinition Create =
                DefineRolePermission("create", "Создание ролей");

            public static readonly PermissionDefinition Read =
                DefineRolePermission("read", "Просмотр ролей");

            public static readonly PermissionDefinition Update =
                DefineRolePermission("update", "Изменение ролей");

            public static readonly PermissionDefinition Delete =
                DefineRolePermission("delete", "Удаление ролей");

            private static PermissionDefinition DefineRolePermission(string action, string displayName) =>
                PermissionDefinition.Create(
                    moduleCode: Identity.Code,
                    moduleDisplayName: Identity.DisplayName,
                    resourceCode: ResourceCode,
                    resourceDisplayName: ResourceName,
                    action: action,
                    displayName: displayName);
        }
    }
}
