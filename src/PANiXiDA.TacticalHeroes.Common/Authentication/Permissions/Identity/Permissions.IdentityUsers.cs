using SectionCatalog = PANiXiDA.TacticalHeroes.Common.Authentication.Sections.Sections;

namespace PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;

public static partial class Permissions
{
    public static class IdentityUsers
    {
        public const string ResourceCode = "users";
        public const string ResourceName = "Пользователи";

        private static readonly Func<string, string, PermissionDefinition> s_define =
            PermissionDefinition.ForResource(
                sectionCode: SectionCatalog.Identity.Code,
                resourceCode: ResourceCode,
                resourceDisplayName: ResourceName);

        public static readonly PermissionDefinition
            Create = s_define("create", "Создание пользователей"),
            Read = s_define("read", "Просмотр пользователей"),
            Update = s_define("update", "Изменение пользователей"),
            Delete = s_define("delete", "Удаление пользователей");
    }
}
