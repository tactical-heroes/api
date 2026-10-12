namespace PANiXiDA.TacticalHeroes.Common.Authentication;

public static partial class Permissions
{
    public static class IdentityRoles
    {
        public const string ResourceCode = "roles";
        public const string ResourceName = "Роли";

        public static readonly ResourceDefinition Resource = new(
            Code: ResourceCode,
            DisplayName: ResourceName,
            SectionCode: Sections.Identity.Code);

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
