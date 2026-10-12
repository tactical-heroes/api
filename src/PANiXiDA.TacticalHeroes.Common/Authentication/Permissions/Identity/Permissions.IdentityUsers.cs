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
            (Create, Read, Update, Delete) = PermissionDefinition.CreateCrud(
                sectionCode: SectionCatalog.Identity.Code,
                resourceCode: ResourceCode,
                resourceDisplayName: ResourceName,
                resourceGenitiveName: "пользователей");
        }
    }
}
