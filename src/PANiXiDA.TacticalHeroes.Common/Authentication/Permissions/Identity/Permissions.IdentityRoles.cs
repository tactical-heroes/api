using SectionCatalog = PANiXiDA.TacticalHeroes.Common.Authentication.Sections.Sections;

namespace PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;

public static partial class Permissions
{
    public static class IdentityRoles
    {
        public const string ResourceCode = "roles";
        public const string ResourceName = "Роли";

        private static readonly Func<string, string, PermissionDefinition> s_define =
            PermissionDefinition.ForResource(
                sectionCode: SectionCatalog.Identity.Code,
                resourceCode: ResourceCode,
                resourceDisplayName: ResourceName);

        public static readonly PermissionDefinition
            Create = s_define("create", "Создание ролей"),
            Read = s_define("read", "Просмотр ролей"),
            Update = s_define("update", "Изменение ролей"),
            Delete = s_define("delete", "Удаление ролей");
    }
}
