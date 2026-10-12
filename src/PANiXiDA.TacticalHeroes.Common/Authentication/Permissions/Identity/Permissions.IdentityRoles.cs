using SectionCatalog = PANiXiDA.TacticalHeroes.Common.Authentication.Sections.Sections;

namespace PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;

public static partial class Permissions
{
    public static class IdentityRoles
    {
        public const string ResourceCode = "roles";
        public const string ResourceName = "Роли";

        public static readonly PermissionDefinition Create =
            Define("create", "Создание ролей");

        public static readonly PermissionDefinition Read =
            Define("read", "Просмотр ролей");

        public static readonly PermissionDefinition Update =
            Define("update", "Изменение ролей");

        public static readonly PermissionDefinition Delete =
            Define("delete", "Удаление ролей");

        private static PermissionDefinition Define(string action, string displayName) =>
            PermissionDefinition.Create(
                sectionCode: SectionCatalog.Identity.Code,
                resourceCode: ResourceCode,
                resourceDisplayName: ResourceName,
                action: action,
                displayName: displayName);
    }
}
