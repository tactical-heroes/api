using SectionCatalog = PANiXiDA.TacticalHeroes.Common.Authentication.Sections.Sections;

namespace PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;

public static partial class Permissions
{
    public static class IdentityUsers
    {
        public const string ResourceCode = "users";
        public const string ResourceName = "Пользователи";

        public static readonly PermissionDefinition Create;
        public static readonly PermissionDefinition Read;
        public static readonly PermissionDefinition Update;
        public static readonly PermissionDefinition Delete;

        static IdentityUsers()
        {
            var define = PermissionDefinition.ForResource(
                sectionCode: SectionCatalog.Identity.Code,
                resourceCode: ResourceCode,
                resourceDisplayName: ResourceName);

            Create = define("create", "Создание пользователей");
            Read = define("read", "Просмотр пользователей");
            Update = define("update", "Изменение пользователей");
            Delete = define("delete", "Удаление пользователей");
        }
    }
}
