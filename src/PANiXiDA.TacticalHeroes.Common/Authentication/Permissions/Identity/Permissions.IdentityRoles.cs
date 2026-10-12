using SectionCatalog = PANiXiDA.TacticalHeroes.Common.Authentication.Sections.Sections;

namespace PANiXiDA.TacticalHeroes.Common.Authentication.Permissions;

public static partial class Permissions
{
    public static class IdentityRoles
    {
        public const string ResourceCode = "roles";
        public const string ResourceName = "Роли";

        public static readonly PermissionDefinition Create;
        public static readonly PermissionDefinition Read;
        public static readonly PermissionDefinition Update;
        public static readonly PermissionDefinition Delete;

        static IdentityRoles()
        {
            (Create, Read, Update, Delete) = PermissionDefinition.CreateCrud(
                sectionCode: SectionCatalog.Identity.Code,
                resourceCode: ResourceCode,
                resourceDisplayName: ResourceName,
                resourceGenitiveName: "ролей");
        }
    }
}
