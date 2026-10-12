namespace PANiXiDA.TacticalHeroes.Common.Authentication;

public static partial class Permissions
{
    public static class IdentityRoles
    {
        public const string ResourceCode = "roles";
        public const string ResourceName = "Роли";

        public static readonly ResourceDefinition Resource =
            Sections.Identity.Definition.CreateResource(ResourceCode, ResourceName);

        public static readonly PermissionDefinition Create =
            Resource.CreatePermission("create", "Создание ролей");

        public static readonly PermissionDefinition Read =
            Resource.CreatePermission("read", "Просмотр ролей");

        public static readonly PermissionDefinition Update =
            Resource.CreatePermission("update", "Изменение ролей");

        public static readonly PermissionDefinition Delete =
            Resource.CreatePermission("delete", "Удаление ролей");
    }
}
