namespace PANiXiDA.TacticalHeroes.Common.Authentication;

public static partial class Permissions
{
    public static class IdentityUsers
    {
        public const string ResourceCode = "users";
        public const string ResourceName = "Пользователи";

        public static readonly ResourceDefinition Resource = new(ResourceCode, ResourceName, Sections.Identity.Code);

        public static readonly PermissionDefinition Create =
            Resource.CreatePermission("create", "Создание пользователей");

        public static readonly PermissionDefinition Read =
            Resource.CreatePermission("read", "Просмотр пользователей");

        public static readonly PermissionDefinition Update =
            Resource.CreatePermission("update", "Изменение пользователей");

        public static readonly PermissionDefinition Delete =
            Resource.CreatePermission("delete", "Удаление пользователей");
    }
}
